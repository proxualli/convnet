#pragma once
#include "Layer.h"

namespace dnn
{
	class Concat final : public Layer
	{
	private:
		std::unique_ptr<dnnl::concat::primitive_desc> fwdDesc;
		std::unordered_map<int, dnnl::memory> fwdArgs;
		std::vector<dnnl::memory::desc> srcsMemsDesc;
#ifdef DNN_CACHE_PRIMITIVES
		std::unique_ptr<dnnl::concat> fwd;
#endif

		auto InputChannels(const std::vector<Layer*>& inputs) const
		{
			auto channels = 0ull;
			for (const auto& layer : inputs)
				channels += layer->C;

			return channels;
		}

		auto IsInputPadded(const std::vector<Layer*>& inputs) const
		{
			auto isPadded = true;
			for (const auto& layer : inputs)
				if (layer->C != layer->PaddedC)
				{
					isPadded = false;
					break;
				}

			return isPadded;
		}

	public:
		FloatArray InputNeurons;
		const bool IsPadded;

		Concat(const dnn::Device& device, const dnnl::memory::format_tag format, const std::string& name, const std::vector<Layer*>& inputs) :
			Layer(device, format, name, LayerTypes::Concat, 0, 0, InputChannels(inputs), inputs[0]->D, inputs[0]->H, inputs[0]->W, 0, 0, 0, inputs),
			InputNeurons(FloatArray()),
			IsPadded(IsInputPadded(inputs))
		{
			assert(Inputs.size() > 1);
		}

		void UpdateResolution() final override
		{
			H = InputLayer->H;
			W = InputLayer->W;
		}

		std::string GetDescription() const final override
		{
			return GetDescriptionHeader();
		}

		UInt FanIn() const final override
		{
			return 1;
		}

		UInt FanOut() const final override
		{
			return 1;
		}

		void SetBatchSize(const UInt batchSize) final override
		{
			Layer::SetBatchSize(batchSize);

			if constexpr (TestConcat)
				InputNeurons.resize(batchSize, C, H, W, dnnl::memory::data_type::f32, BlockedFmt, Device.engine);
		}

		void InitializeDescriptors(const UInt batchSize) final override
		{
			if (GetMemoryNDims(*InputLayer->DstMemDesc) == 2)
			{
				ChosenFormat = dnnl::memory::format_tag::nc;
				
				DstMemDesc = std::make_unique<dnnl::memory::desc>(dnnl::memory::desc(dnnl::memory::dims({ dnnl::memory::dim(batchSize), dnnl::memory::dim(C) }), dnnl::memory::data_type::f32, ChosenFormat));
				DiffDstMemDesc = std::make_unique<dnnl::memory::desc>(dnnl::memory::desc(dnnl::memory::dims({ dnnl::memory::dim(batchSize), dnnl::memory::dim(C) }), dnnl::memory::data_type::f32, ChosenFormat));
			}
			else
			{
				if (NeuronsFormat == dnnl::memory::format_tag::any)
				{
					ChosenFormat = GetMemoryFormat(*InputLayer->DstMemDesc);
					if (ChosenFormat != GetMemoryFormat(*InputLayer->DiffDstMemDesc))
						throw std::invalid_argument("Src and Diff format are different in " + std::string(magic_enum::enum_name<LayerTypes>(LayerType)) + " layer " + Name);
				}
				else
					ChosenFormat = PlainFmt;

				DstMemDesc = std::make_unique<dnnl::memory::desc>(dnnl::memory::desc(dnnl::memory::dims({ dnnl::memory::dim(batchSize), dnnl::memory::dim(C), dnnl::memory::dim(H), dnnl::memory::dim(W) }), dnnl::memory::data_type::f32, ChosenFormat));
				DiffDstMemDesc = std::make_unique<dnnl::memory::desc>(dnnl::memory::desc(dnnl::memory::dims({ dnnl::memory::dim(batchSize), dnnl::memory::dim(C), dnnl::memory::dim(H), dnnl::memory::dim(W) }), dnnl::memory::data_type::f32, ChosenFormat));
			}

			for (auto i = 1ull; i < Inputs.size(); i++)
			{
				assert(ChosenFormat == GetMemoryFormat(*Inputs[i]->DstMemDesc));
				if (ChosenFormat != GetMemoryFormat(*Inputs[i]->DstMemDesc))
					throw std::invalid_argument("Incompatible memory formats in " + std::string(magic_enum::enum_name<LayerTypes>(LayerType)) + " layer " + Inputs[i]->Name);
			}

			srcsMemsDesc = std::vector<dnnl::memory::desc>();
			for (auto i = 0ull; i < Inputs.size(); i++)
			{
				if (GetMemoryNDims(*Inputs[i]->DstMemDesc) == 2)
					srcsMemsDesc.push_back(dnnl::memory::desc(dnnl::memory::dims({ dnnl::memory::dim(batchSize), dnnl::memory::dim(Inputs[i]->C) }), dnnl::memory::data_type::f32, ChosenFormat));
				else
					srcsMemsDesc.push_back(dnnl::memory::desc(dnnl::memory::dims({ dnnl::memory::dim(batchSize), dnnl::memory::dim(Inputs[i]->C), dnnl::memory::dim(Inputs[i]->H), dnnl::memory::dim(Inputs[i]->W) }), dnnl::memory::data_type::f32, ChosenFormat));
			}

			fwdDesc = std::make_unique<dnnl::concat::primitive_desc>(dnnl::concat::primitive_desc(Device.engine, *DstMemDesc, 1, srcsMemsDesc));

			fwdArgs = std::unordered_map<int, dnnl::memory>{ { DNNL_ARG_DST, dnnl::memory(*DstMemDesc, Device.engine, Neurons.data()) } };
			for (auto i = 0ull; i < InputsFwd.size(); i++)
				fwdArgs.insert({ DNNL_ARG_MULTIPLE_SRC + int(i), dnnl::memory(srcsMemsDesc[i], Device.engine, Inputs[i]->Neurons.data())});

#ifdef DNN_CACHE_PRIMITIVES
			fwd = std::make_unique<dnnl::concat>(dnnl::concat(*fwdDesc));
#endif
		}

		void ForwardProp(const UInt batchSize, const bool training) final override
		{
			if (training)
			{
#ifdef DNN_LEAN
				DNN_UNREF_PAR(batchSize);

#ifdef DNN_CACHE_PRIMITIVES
				fwd->execute(Device.stream, fwdArgs);
#else
				dnnl::concat(*fwdDesc).execute(Device.stream, fwdArgs);
#endif
				Device.stream.wait();
#else
				if constexpr (!Reference)
				{
					const auto plain = IsPlainFormat();
					const auto threads = GetThreads(batchSize * (plain ? CDHW() : PaddedCDHW()), Float(10));
					const auto strideHW = HW() * VectorSize;

#ifdef DNN_STOCHASTIC
					if (batchSize == 1)
					{
						if (!plain && IsPadded)
						{
							VecFloat In;
							auto channelOffset = 0ull;
							UInt inputIndex, outputIndex;
							for (auto inputLayer = 0ull; inputLayer < Inputs.size(); inputLayer++)
							{
								for (auto c = channelOffset; c < channelOffset + Inputs[inputLayer]->PaddedC; c += VectorSize)
								{
									inputIndex = (c - channelOffset) * HW();
									outputIndex = c * HW();
									for (auto hw = 0ull; hw < strideHW; hw += VectorSize)
									{
										In.load_a(&Inputs[inputLayer]->Neurons[hw + inputIndex]);
										In.store_a(&Neurons[hw + outputIndex]);
#ifndef DNN_LEAN
										VecZero.store_nt(&NeuronsD1[hw + outputIndex]);
#endif
									}
								}
								channelOffset += Inputs[inputLayer]->PaddedC;
							}
						}
						else
						{
							auto channelOffset = 0ull;
							UInt inputIndex, outputIndex;
							for (auto inputLayer = 0ull; inputLayer < Inputs.size(); inputLayer++)
							{
								for (auto c = channelOffset; c < channelOffset + Inputs[inputLayer]->C; c++)
								{
									inputIndex = (c - channelOffset) * HW();
									outputIndex = c * HW();
									PRAGMA_OMP_SIMD()
									for (auto hw = 0ull; hw < HW(); hw++)
									{
										Neurons[outputIndex + hw] = Inputs[inputLayer]->Neurons[inputIndex + hw];
#ifndef DNN_LEAN
										NeuronsD1[outputIndex + hw] = Float(0);
#endif
									}
								}
								channelOffset += Inputs[inputLayer]->C;
							}
						}
					}
					else
					{
#endif
						if (!plain)
						{
							if (IsPadded)
							{
								for_i(batchSize, threads, [=](UInt n)
								{
									const auto outputSampleOffset = n * PaddedCDHW();
									auto channelOffset = 0ull;
									UInt inputIndex, outputIndex;
									VecFloat In;
									for (auto inputLayer = 0ull; inputLayer < Inputs.size(); inputLayer++)
									{
										const auto inputSampleOffset = n * Inputs[inputLayer]->PaddedCDHW();
										for (auto c = channelOffset; c < channelOffset + Inputs[inputLayer]->PaddedC; c += VectorSize)
										{
											inputIndex = ((c - channelOffset) * HW()) + inputSampleOffset;
											outputIndex = (c * HW()) + outputSampleOffset;
											for (auto hw = 0ull; hw < strideHW; hw += VectorSize)
											{
												In.load_a(&Inputs[inputLayer]->Neurons[hw + inputIndex]);
												In.store_a(&Neurons[hw + outputIndex]);
#ifndef DNN_LEAN
												VecZero.store_nt(&NeuronsD1[hw + outputIndex]);
#endif
											}
										}
										channelOffset += Inputs[inputLayer]->PaddedC;
									}
								});
							}
							else
							{
								/*for_i(batchSize, threads, [=](UInt n)
								{
									auto channelOffset = 0ull;
									for (auto i = 0ull; i < Inputs.size(); i++)
									{
										for (auto c = 0ull; c < Inputs[i]->C; c++)
										{
											for (auto h = 0ull; h < H; h++)
												for (auto w = 0ull; w < W; w++)
												{
													Neurons[OffsetPaddedMem(n, c + channelOffset, h, w)] = Inputs[i]->Neurons[Inputs[i]->OffsetPaddedMem(n, c, h, w)];
#ifndef DNN_LEAN
													NeuronsD1[OffsetPaddedMem(n, c + channelOffset, h, w)] = Float(0);
#endif
												}
										}
										channelOffset += Inputs[i]->C;
									}
								});*/

								for_i(batchSize, threads, [=](UInt n)
								{
									const auto outputSampleOffset = n * PaddedCDHW();
									auto channelOffset = 0ull;
									UInt inputIndex, outputIndex;
									VecFloat In;
									for (auto inputLayer = 0ull; inputLayer < Inputs.size(); inputLayer++)
									{
										const auto inputSampleOffset = n * Inputs[inputLayer]->PaddedCDHW();
										for (auto c = channelOffset; c < channelOffset + (Inputs[inputLayer]->PaddedC - VectorSize); c += VectorSize)
										{
											inputIndex = ((c - channelOffset) * HW()) + inputSampleOffset;
											outputIndex = (c * HW()) + outputSampleOffset;
											for (auto hw = 0ull; hw < strideHW; hw += VectorSize)
											{
												In.load_a(&Inputs[inputLayer]->Neurons[hw + inputIndex]);
												In.store_a(&Neurons[hw + outputIndex]);
#ifndef DNN_LEAN
												VecZero.store_nt(&NeuronsD1[hw + outputIndex]);
#endif
											}
										}
										inputIndex = (((Inputs[inputLayer]->PaddedC - VectorSize) - channelOffset) * HW()) + inputSampleOffset;
										outputIndex = ((Inputs[inputLayer]->PaddedC - VectorSize) * HW()) + outputSampleOffset;
										const auto len = VectorSize - (Inputs[inputLayer]->PaddedC - Inputs[inputLayer]->C);
										auto hwIn = 0ull;
										for (auto hw = 0ull; hw < strideHW; hw += VectorSize)
										{
											In.load_a(&Inputs[inputLayer]->Neurons[hwIn + inputIndex]);
											In.cutoff(static_cast<int>(len));
											In.store_a(&Neurons[hw + outputIndex]);
#ifndef DNN_LEAN
											VecZero.store_nt(&NeuronsD1[hw + outputIndex]);
#endif
											hwIn += len;
										}
										channelOffset += Inputs[inputLayer]->C;
									}
								});
							}
						}
						else
							for_i(batchSize, threads, [=](UInt n)
							{
								const auto outputSampleOffset = n * CDHW();
								auto channelOffset = 0ull;
								UInt inputIndex, outputIndex;
								for (auto inputLayer = 0ull; inputLayer < Inputs.size(); inputLayer++)
								{
									const auto inputSampleOffset = n * Inputs[inputLayer]->CDHW();
									for (auto c = channelOffset; c < channelOffset + Inputs[inputLayer]->C; c++)
									{
										inputIndex = ((c - channelOffset) * HW()) + inputSampleOffset;
										outputIndex = (c * HW()) + outputSampleOffset;
										PRAGMA_OMP_SIMD()
										for (auto hw = 0ull; hw < HW(); hw++)
										{
											Neurons[outputIndex + hw] = Inputs[inputLayer]->Neurons[inputIndex + hw];
#ifndef DNN_LEAN
											NeuronsD1[outputIndex + hw] = Float(0);
#endif
										}
									}
									channelOffset += Inputs[inputLayer]->C;
								}
							});
#ifdef DNN_STOCHASTIC
					}
#endif
					if constexpr (TestConcat)
					{
						fwdArgs = std::unordered_map<int, dnnl::memory>{ { DNNL_ARG_DST, dnnl::memory(*DstMemDesc, Device.engine, InputNeurons.data()) } };
						for (auto i = 0ull; i < InputsFwd.size(); i++)
							fwdArgs.insert({ DNNL_ARG_MULTIPLE_SRC + int(i), dnnl::memory(srcsMemsDesc[i], Device.engine, Inputs[i]->Neurons.data()) });

						for (auto i = 0ull; i < InputNeurons.size(); i++)
							InputNeurons[i] = Float(0);

#ifdef DNN_CACHE_PRIMITIVES
						fwd->execute(Device.stream, fwdArgs);
#else
						dnnl::concat(*fwdDesc).execute(Device.stream, fwdArgs);
#endif
						Device.stream.wait();


						const auto margin = Float(0.0001);

						for (auto i = 0ull; i < Neurons.size(); i++)
						{
							if (((InputNeurons[i] - margin) > Neurons[i]) || ((InputNeurons[i] + margin) < Neurons[i]))
							{
								cimg_library::cimg::dialog("Concat Sanity Check", (std::string("Forward Check not passed: ") + Name).c_str(), "OK");
								break;
							}

							if (NeuronsD1[i] != Float(0))
							{
								cimg_library::cimg::dialog("Concat Sanity Check", (std::string("Forward Check D1 not passed: ") + Name).c_str(), "OK");
								break;
							}
						}
					}
				}
				else
				{
#ifdef DNN_CACHE_PRIMITIVES
					fwd->execute(Device.stream, fwdArgs);
#else
					dnnl::concat(*fwdDesc).execute(Device.stream, fwdArgs);
#endif
					Device.stream.wait();

					InitArray<Float>(NeuronsD1.data(), batchSize * PaddedCDHW());
				}
#endif
			}
			else
			{
#ifdef DNN_CACHE_PRIMITIVES
				fwd->execute(Device.stream, fwdArgs);
#else
				dnnl::concat(*fwdDesc).execute(Device.stream, fwdArgs);
#endif
				Device.stream.wait();
			}
		}

		void BackwardProp(const UInt batchSize) final override
		{
#ifdef DNN_LEAN
			ZeroGradientMulti(batchSize);
#endif // DNN_LEAN

			const auto plain = IsPlainFormat();
			const auto threads = GetThreads(batchSize * (plain ? CDHW() : PaddedCDHW()), Float(10));
			const auto strideHW = HW() * VectorSize;

#ifdef DNN_STOCHASTIC
			if (batchSize == 1)
			{
				if (!plain && IsPadded)
				{
					auto channelOffset = 0ull;
					UInt inputIndex, outputIndex;
					VecFloat inputD1, D1;
					for (auto inputLayer = 0ull; inputLayer < Inputs.size(); inputLayer++)
					{
						for (auto c = channelOffset; c < channelOffset + Inputs[inputLayer]->PaddedC; c += VectorSize)
						{
							inputIndex = ((c - channelOffset) * HW());
							outputIndex = (c * HW());
							for (auto hw = 0ull; hw < strideHW; hw += VectorSize)
							{
								inputD1.load_a(&Inputs[inputLayer]->NeuronsD1[hw + inputIndex]);
								D1.load_a(&NeuronsD1[hw + outputIndex]);
								inputD1 += D1;
								inputD1.store_a(&Inputs[inputLayer]->NeuronsD1[hw + inputIndex]);
							}
						}									
						channelOffset += Inputs[inputLayer]->PaddedC;
					}
				}
				else
				{
					auto channelOffset = 0ull;
					UInt inputIndex, outputIndex;
					for (auto inputLayer = 0ull; inputLayer < Inputs.size(); inputLayer++)
					{
						for (auto c = channelOffset; c < channelOffset + Inputs[inputLayer]->C; c++)
						{
							inputIndex = ((c - channelOffset) * HW());
							outputIndex = (c * HW());
							PRAGMA_OMP_SIMD()
							for (auto hw = 0ull; hw < HW(); hw++)
								Inputs[inputLayer]->NeuronsD1[inputIndex + hw] += NeuronsD1[outputIndex + hw];
						}
						channelOffset += Inputs[inputLayer]->C;
					}
				}
			}
			else
			{
#endif
				if (!plain)
				{
					if (IsPadded)
					{
						for_i(batchSize, threads, [=](UInt n)
						{
							const auto outputSampleOffset = n * PaddedCDHW();
							auto channelOffset = 0ull;
							UInt inputIndex, outputIndex;
							VecFloat inputD1, D1;
							for (auto inputLayer = 0ull; inputLayer < Inputs.size(); inputLayer++)
							{
								const auto inputSampleOffset = n * Inputs[inputLayer]->PaddedCDHW();
								for (auto c = channelOffset; c < channelOffset + Inputs[inputLayer]->PaddedC; c += VectorSize)
								{
									inputIndex = ((c - channelOffset) * HW()) + inputSampleOffset;
									outputIndex = (c * HW()) + outputSampleOffset;
									for (auto hw = 0ull; hw < strideHW; hw += VectorSize)
									{
										inputD1.load_a(&Inputs[inputLayer]->NeuronsD1[hw + inputIndex]);
										D1.load_a(&NeuronsD1[hw + outputIndex]);
										inputD1 += D1;
										inputD1.store_a(&Inputs[inputLayer]->NeuronsD1[hw + inputIndex]);
									}
								}
								channelOffset += Inputs[inputLayer]->PaddedC;
							}
						});
					}
					else
					{
						/*for_i(batchSize, threads, [=](UInt n)
						{
							auto channelOffset = 0ull;
							for (auto i = 0ull; i < Inputs.size(); i++)
							{
								for (auto c = 0ull; c < Inputs[i]->C; c++)
									for (auto h = 0ull; h < H; h++)
										for (auto w = 0ull; w < W; w++)
											Inputs[i]->NeuronsD1[Inputs[i]->OffsetPaddedMem(n, c, h, w)] += NeuronsD1[OffsetPaddedMem(n, c + channelOffset, h, w)];

								channelOffset += Inputs[i]->C;
							}
						});*/

						for_i(batchSize, threads, [=](UInt n)
						{
							const auto outputSampleOffset = n * PaddedCDHW();
							auto channelOffset = 0ull;
							UInt inputIndex, outputIndex;
							VecFloat inputD1, D1;
							for (auto inputLayer = 0ull; inputLayer < Inputs.size(); inputLayer++)
							{
								const auto inputSampleOffset = n * Inputs[inputLayer]->PaddedCDHW();
								for (auto c = channelOffset; c < channelOffset + (Inputs[inputLayer]->PaddedC - VectorSize); c += VectorSize)
								{
									inputIndex = ((c - channelOffset) * HW()) + inputSampleOffset;
									outputIndex = (c * HW()) + outputSampleOffset;
									for (auto hw = 0ull; hw < strideHW; hw += VectorSize)
									{
										inputD1.load_a(&Inputs[inputLayer]->NeuronsD1[hw + inputIndex]);
										D1.load_a(&NeuronsD1[hw + outputIndex]);
										inputD1 += D1;
										inputD1.store_a(&Inputs[inputLayer]->NeuronsD1[hw + inputIndex]);
									}
								}
								inputIndex = (((Inputs[inputLayer]->PaddedC - VectorSize) - channelOffset) * HW()) + inputSampleOffset;
								outputIndex = ((Inputs[inputLayer]->PaddedC - VectorSize) * HW()) + outputSampleOffset;
								const auto len = VectorSize - (Inputs[inputLayer]->PaddedC - Inputs[inputLayer]->C);
								auto hwIn = 0ull;
								for (auto hw = 0ull; hw < strideHW; hw += VectorSize)
								{
									inputD1.load_a(&Inputs[inputLayer]->NeuronsD1[hwIn + inputIndex]);
									D1.load_a(&NeuronsD1[hw + outputIndex]);
									inputD1 += D1;
									inputD1.cutoff(static_cast<int>(len));
									inputD1.store_a(&Inputs[inputLayer]->NeuronsD1[hwIn + inputIndex]);
									hwIn += len;
								}
								channelOffset += Inputs[inputLayer]->C;
							}
						});
					}
				}
				else
					for_i(batchSize, threads, [=](UInt n)
					{
						const auto outputSampleOffset = n * CDHW();
						auto channelOffset = 0ull;
						UInt inputIndex, outputIndex;
						for (auto inputLayer = 0ull; inputLayer < Inputs.size(); inputLayer++)
						{
							const auto inputSampleOffset = n * Inputs[inputLayer]->CDHW();
							for (auto c = channelOffset; c < channelOffset + Inputs[inputLayer]->C; c++)
							{
								inputIndex = ((c - channelOffset) * HW()) + inputSampleOffset;
								outputIndex = (c * HW()) + outputSampleOffset;
								PRAGMA_OMP_SIMD()
								for (auto hw = 0ull; hw < HW(); hw++)
									Inputs[inputLayer]->NeuronsD1[inputIndex + hw] += NeuronsD1[outputIndex + hw];
							}
							channelOffset += Inputs[inputLayer]->C;
						}
					});
#ifdef DNN_STOCHASTIC
			}
#endif

#ifdef DNN_LEAN
			ReleaseGradient();
#endif // DNN_LEAN
		}
	};
}