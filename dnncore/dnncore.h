#pragma once

#include <string>
#include <vcclr.h>
#include <msclr/marshal.h>
#include <msclr/marshal_cppstd.h>
#include <cliext/vector>   

using namespace System;
using namespace System::ComponentModel;
using namespace System::Runtime::InteropServices;
using namespace System::Timers;
using namespace System::Threading;
using namespace System::Text;
using namespace System::IO;
using namespace System::IO::Compression;
using namespace System::Xml;
using namespace System::Xml::Schema;
using namespace System::Xml::Serialization;
using namespace System::Diagnostics;
using namespace System::Threading::Tasks;
using namespace System::Windows::Media;
using namespace msclr::interop;

namespace dnncore 
{
	typedef float Float;
	typedef unsigned char Byte;

	constexpr Byte FloatSaturate(const Float value) { return value > Float(255) ? Byte(255) : value < Float(0) ? Byte(0) : Byte(value); }

	inline static std::string ToUnmanagedString(String^ s)
	{
		return msclr::interop::marshal_as<std::string>(s);
	}

	inline static String^ ToManagedString(const std::string& s) { return gcnew String(s.c_str()); }

	[Serializable()]
	public enum class DNNAlgorithms
	{
		Linear = 0,
		Nearest = 1
	};

	[Serializable()]
	public enum class DNNInterpolation
	{
		Cubic = 0,
		Linear = 1,
		Nearest = 2
	};

	[Serializable()]
	public enum class DNNOptimizers
	{
		AdaDelta = 0,
		AdaGrad = 1,
		Adam = 2,
		Adamax = 3,
		NAG = 4,
		RMSProp = 5,
		SGD = 6,
		SGDMomentum = 7,
		RAdam = 8
	};

	[Serializable()]
	public enum class DNNDatasets
	{
		cifar10 = 0,
		cifar100 = 1,
		fashionmnist = 2,
		mnist = 3,
		tinyimagenet = 4
	};


	[Serializable()]
	public enum class DNNScripts
	{
		densenet = 0,
		mobilenetv3 = 1,
		resnet = 2,
		shufflenetv2 = 3
	};

	[Serializable()]
	public enum class DNNCosts
	{
		BinaryCrossEntropy = 0,
		CategoricalCrossEntropy = 1,
		MeanAbsoluteEpsError = 2,
		MeanAbsoluteError = 3,
		MeanSquaredError = 4,
		SmoothHinge = 5
	};

	[Serializable()]
	public enum class DNNFillers
	{
		Constant = 0,
		HeNormal = 1,
		HeUniform = 2,
		LeCunNormal = 3,
		LeCunUniform = 4,
		Normal = 5,
		TruncatedNormal = 6,
		Uniform = 7,
		XavierNormal = 8,
		XavierUniform = 9
	};

	[Serializable()]
	public enum class DNNLayerTypes
	{
		Activation = 0,
		Add = 1,
		Average = 2,
		AvgPooling = 3,
		BatchNorm = 4,
		BatchNormHardLogistic = 5,
		BatchNormHardSwish = 6,
		BatchNormHardSwishDropout = 7,
		BatchNormMish = 8,
		BatchNormMishDropout = 9,
		BatchNormRelu = 10,
		BatchNormReluDropout = 11,
		BatchNormSwish = 12,
		ChannelMultiply = 13,
		ChannelShuffle = 14,
		ChannelSplit = 15,
		ChannelZeroPad = 16,
		Concat = 17,
		Convolution = 18,
		ConvolutionTranspose = 19,
		Cost = 20,
		Dense = 21,
		DepthwiseConvolution = 22,
		Divide = 23,
		Dropout = 24,
		GlobalAvgPooling = 25,
		GlobalMaxPooling = 26,
		Input = 27,
		LocalResponseNormalization = 28,
		Max = 29,
		MaxPooling = 30,
		Min = 31,
		Multiply = 32,
		PartialDepthwiseConvolution = 33,
		Resampling = 34,
		Substract = 35
	};

	[Serializable()]
	public enum class DNNActivations
	{
		Abs = 0,
		BoundedRelu = 1,
		Clip = 2,
		ClipV2 = 3,
		Elu = 4,
		Exp = 5,
		FTS = 6,
		Gelu = 7,
		GeluErf = 8,
		HardLogistic = 9,
		HardSwish = 10,
		Linear = 11,
		Log = 12,
		Logistic = 13,
		LogLogistic = 14,
		LogSoftmax = 15,
		Mish = 16,
		Pow = 17,
		PRelu = 18,
		Relu = 19,
		Round = 20,
		Softmax = 21,
		SoftRelu = 22,
		Sqrt = 23,
		Square = 24,
		Swish = 25,
		Tanh = 26
	};

	[Serializable()]
	public enum class DNNStates
	{
		Idle = 0,
		NewEpoch = 1,
		Testing = 2,
		Training = 3,
		SaveWeights = 4,
		Completed = 5
	};

	[Serializable()]
	public enum class DNNTaskStates
	{
		Paused = 0,
		Running = 1,
		Stopped = 2
	};

	/*
	[Serializable()]
	public ref class DNNModelParameters : public System::ComponentModel::INotifyPropertyChanged
	{
	public:
		property System::Array^ Scripts
		{
			System::Array^ get() { return Enum::GetValues(DNNScripts::typeid); }
		}

		property System::Array^ Datasets
		{
			System::Array^ get() { return Enum::GetValues(DNNDatasets::typeid); }
		}

		property System::Array^ Fillers
		{
			System::Array^ get() { return Enum::GetValues(DNNFillers::typeid); }
		}

		property String^ ModelName
		{
			String^ get()
			{
				switch (Script)
				{
				case DNNScripts::densenet:
					ModelName = Script.ToString() + "-" + H.ToString() + "x" + W.ToString() + "-" + Groups.ToString() + "-" + Iterations.ToString() + "-" + GrowthRate.ToString() + (Dropout > 0.0f ? "-dropout" : "") + (Compression > 0.0f ? "-compression" : "") + (Bottleneck ? "-bottleneck" : "");
					break;
				case DNNScripts::mobilenetv3:
					ModelName = Script.ToString() + "-" + H.ToString() + "x" + W.ToString() + "-" + Groups.ToString() + "-" + Iterations.ToString() + "-" + Width.ToString() + (SqueezeExcitation ? "-se" : "");
					break;
				case DNNScripts::resnet:
					ModelName = Script.ToString() + "-" + H.ToString() + "x" + W.ToString() + "-" + Groups.ToString() + "-" + Iterations.ToString() + "-" + Width.ToString() + (Dropout > 0.0f ? "-dropout" : "") + (Bottleneck ? "-bottleneck" : "") + (ChannelZeroPad ? "-channelzeropad" : "");
					break;
				case DNNScripts::shufflenetv2:
					ModelName = Script.ToString() + "-" + H.ToString() + "x" + W.ToString() + "-" + Groups.ToString() + "-" + Iterations.ToString() + "-" + Width.ToString();
					break;
				}
				return modelName;
			}

			void set(String^ value)
			{
				if (value == nullptr || modelName == nullptr)
				{
					modelName = value;
					OnPropertyChanged("ModelName");
				}
				else
				{
					if (!value->Equals(modelName))
					{
						modelName = value;
						OnPropertyChanged("ModelName");
					}
				}
			}
		}
		property DNNScripts Script
		{
			DNNScripts get() { return model; }
			void set(DNNScripts value)
			{
				if (value == model)
					return;

				model = value;
				OnPropertyChanged("Script");
				OnPropertyChanged("Depth");
				OnPropertyChanged("WidthVisible");
				OnPropertyChanged("GrowthRateVisible");
				OnPropertyChanged("DropoutVisible");
				OnPropertyChanged("CompressionVisible");
				OnPropertyChanged("BottleneckVisible");
				OnPropertyChanged("ChannelZeroPadVisible");
				OnPropertyChanged("SqueezeExcitationVisible");
				OnPropertyChanged("BottleneckVisible");
			}
		}

		property DNNDatasets Dataset
		{
			DNNDatasets get() { return dataset; }
			void set(DNNDatasets value)
			{
				if (value == dataset)
					return;

				dataset = value;
				OnPropertyChanged("Dataset");

				switch (dataset)
				{
				case DNNDatasets::cifar10:
					C = 3;
					break;

				case DNNDatasets::cifar100:
					C = 3;
					break;

				case DNNDatasets::fashionmnist:
				case DNNDatasets::mnist:
					C = 1;
					break;

				case DNNDatasets::tinyimagenet:
					C = 3;
					break;
				}
			}
		}

		property size_t Classes
		{
			size_t get()
			{
				switch (Dataset)
				{
				case DNNDatasets::cifar100:
					return 100;
				case DNNDatasets::tinyimagenet:
					return 200;
				default:
					return 10;
				}
			}
		}

		property size_t C
		{
			size_t get() { return c; }
			void set(size_t value)
			{
				if (value != c)
				{
					c = value;
					OnPropertyChanged("C");
				}
			}
		}
		property size_t D
		{
			size_t get() { return 1; }
		}
		property size_t H
		{
			size_t get() { return h; }
			void set(size_t value)
			{
				if (value >= 14 && value <= 256 && value != h)
				{
					h = value;
					OnPropertyChanged("H");
				}
			}
		}
		property size_t W
		{
			size_t get() { return w; }
			void set(size_t value)
			{
				if (value >= 14 && value <= 256 && value != w)
				{
					w = value;
					OnPropertyChanged("W");
				}
			}
		}
		property size_t PadD
		{
			size_t get() { return 0; }
		}
		property size_t PadH
		{
			size_t get() { return padH; }
			void set(size_t value)
			{
				if (value <= H && value != padH)
				{
					padH = value;
					OnPropertyChanged("PadH");
					OnPropertyChanged("HasPadding");
				}
			}
		}
		property size_t PadW
		{
			size_t get() { return padW; }
			void set(size_t value)
			{
				if (value <= W && value != padW)
				{
					padW = value;
					OnPropertyChanged("PadW");
					OnPropertyChanged("HasPadding");
				}
			}
		}
		property bool MirrorPad
		{
			bool get() { return mirrorPad; }
			void set(bool value)
			{
				if (value == mirrorPad)
					return;

				mirrorPad = value;
				OnPropertyChanged("MirrorPad");
			}
		}
		property bool HasPadding
		{
			bool get() { return PadH > 0 || PadW > 0; }
		}
		property bool RandomCrop
		{
			bool get() { return HasPadding; }
		}
		property bool MeanStdNormalization
		{
			bool get() { return meanStdNormalization; }
			void set(bool value)
			{
				if (value == meanStdNormalization)
					return;

				meanStdNormalization = value;
				OnPropertyChanged("MeanStdNormalization");
			}
		}
		property DNNFillers WeightsFiller
		{
			DNNFillers get() { return weightsFiller; }
			void set(DNNFillers value)
			{
				if (value == weightsFiller)
					return;

				weightsFiller = value;
				OnPropertyChanged("WeightsFiller");
				OnPropertyChanged("WeightsScaleVisible");
			}
		}
		property bool WeightsScaleVisible
		{
			bool get()
			{
				return WeightsFiller == DNNFillers::Constant || WeightsFiller == DNNFillers::Normal || WeightsFiller == DNNFillers::TruncatedNormal || WeightsFiller == DNNFillers::Uniform;
			}
		}
		property Float WeightsScale
		{
			Float get() { return weightsScale; }
			void set(Float value)
			{
				if (value == weightsScale)
					return;

				weightsScale = value;
				OnPropertyChanged("WeightsScale");
			}
		}
		property Float WeightsLRM
		{
			Float get() { return weightsLRM; }
			void set(Float value)
			{
				if (value == weightsLRM)
					return;

				weightsLRM = value;
				OnPropertyChanged("WeightsLRM");
			}
		}
		property Float WeightsWDM
		{
			Float get() { return weightsWDM; }
			void set(Float value)
			{
				if (value == weightsWDM)
					return;

				weightsWDM = value;
				OnPropertyChanged("WeightsWDM");
			}
		}
		property bool HasBias
		{
			bool get() { return hasBias; }
			void set(bool value)
			{
				if (value == hasBias)
					return;

				hasBias = value;
				OnPropertyChanged("HasBias");
			}
		}
		property DNNFillers BiasesFiller
		{
			DNNFillers get() { return biasesFiller; }
			void set(DNNFillers value)
			{
				if (value == biasesFiller)
					return;

				biasesFiller = value;
				OnPropertyChanged("BiasesFiller");
				OnPropertyChanged("BiasesScaleVisible");
			}
		}
		property bool BiasesScaleVisible
		{
			bool get()
			{
				return BiasesFiller == DNNFillers::Constant || BiasesFiller == DNNFillers::Normal || BiasesFiller == DNNFillers::TruncatedNormal || BiasesFiller == DNNFillers::Uniform;
			}
		}
		property Float BiasesScale
		{
			Float get() { return biasesScale; }
			void set(Float value)
			{
				if (value == biasesScale)
					return;

				biasesScale = value;
				OnPropertyChanged("BiasesScale");
			}
		}
		property Float BiasesLRM
		{
			Float get() { return biasesLRM; }
			void set(Float value)
			{
				if (value == biasesLRM)
					return;

				biasesLRM = value;
				OnPropertyChanged("BiasesLRM");
			}
		}
		property Float BiasesWDM
		{
			Float get() { return biasesWDM; }
			void set(Float value)
			{
				if (value == biasesWDM)
					return;

				biasesWDM = value;
				OnPropertyChanged("BiasesWDM");
			}
		}
		property Float BatchNormMomentum
		{
			Float get() { return batchNormMomentum; }
			void set(Float value)
			{
				if (value > 0 && value <= 1 && value != batchNormMomentum)
				{
					batchNormMomentum = value;
					OnPropertyChanged("BatchNormMomentum");
				}
			}
		}
		property Float BatchNormEps
		{
			Float get() { return batchNormEps; }
			void set(Float value)
			{
				if (value == batchNormEps)
					return;

				batchNormEps = value;
				OnPropertyChanged("BatchNormEps");
			}
		}
		property bool BatchNormScaling
		{
			bool get() { return batchNormScaling; }
			void set(bool value)
			{
				if (value == batchNormScaling)
					return;

				batchNormScaling = value;
				OnPropertyChanged("BatchNormScaling");
			}
		}
		property Float Alpha
		{
			Float get() { return alpha; }
			void set(Float value)
			{
				if (value >= 0.0f && value <= 1.0f && value != alpha)
				{
					alpha = value;
					OnPropertyChanged("Alpha");
				}
			}
		}
		property Float Beta
		{
			Float get() { return beta; }
			void set(Float value)
			{
				if (value >= 0.0f && value <= 1.0f && value != beta)
				{
					beta = value;
					OnPropertyChanged("Beta");
				}
			}
		}
		property size_t Groups
		{
			size_t get() { return groups; }
			void set(size_t value)
			{
				if (value == groups)
					return;

				groups = value;
				OnPropertyChanged("Groups");
				OnPropertyChanged("Depth");
			}
		}
		property size_t Iterations
		{
			size_t get() { return iterations; }
			void set(size_t value)
			{
				if (value == iterations)
					return;

				iterations = value;
				OnPropertyChanged("Iterations");
				OnPropertyChanged("Depth");
			}
		}
		property size_t Depth
		{
			size_t get()
			{
				switch (Script)
				{
				case DNNScripts::densenet:
					return (Groups * Iterations * (Bottleneck ? 2u : 1u)) + ((Groups - 1) * 2u);

				case DNNScripts::mobilenetv3:
					return (Groups * Iterations * 3u) + ((Groups - 1) * 2u);

				case DNNScripts::resnet:
					return (Groups * Iterations * (Bottleneck ? 3u : 2u)) + ((Groups - 1) * 2u);

				case DNNScripts::shufflenetv2:
					return (Groups * (Iterations - 1) * 3u) + (Groups * 5u) + 1u;
				default:
					return (Groups * (Iterations - 1) * 3u) + (Groups * 5u) + 1u;
				}
			}
		}

		property size_t Width
		{
			size_t get() { return width; }
			void set(size_t value)
			{
				if (value == width)
					return;

				width = value;
				OnPropertyChanged("Width");
			}
		}
		property bool WidthVisible
		{
			bool get()
			{
				return Script == DNNScripts::mobilenetv3 || Script == DNNScripts::resnet || Script == DNNScripts::shufflenetv2;
			}
		}
		property size_t GrowthRate
		{
			size_t get() { return growthRate; }
			void set(size_t value)
			{
				if (value == growthRate)
					return;

				growthRate = value;
				OnPropertyChanged("GrowthRate");
			}
		}
		property bool GrowthRateVisible
		{
			bool get()
			{
				return Script == DNNScripts::densenet;
			}
		}
		property bool Bottleneck
		{
			bool get() { return bottleneck; }
			void set(bool value)
			{
				if (value == bottleneck)
					return;

				bottleneck = value;
				OnPropertyChanged("Bottleneck");
				OnPropertyChanged("Depth");
			}
		}
		property bool BottleneckVisible
		{
			bool get()
			{
				return Script == DNNScripts::densenet || Script == DNNScripts::resnet;
			}
		}
		property Float Dropout
		{
			Float get() { return dropout; }
			void set(Float value)
			{
				if (value >= 0 && value < 1 && value != dropout)
				{
					dropout = value;
					OnPropertyChanged("Dropout");
				}
			}
		}
		property bool DropoutVisible
		{
			bool get()
			{
				return Script == DNNScripts::densenet || Script == DNNScripts::resnet;
			}
		}
		property Float Compression
		{
			Float get() { return compression; }
			void set(Float value)
			{
				if (value >= 0 && value <= 1 && value != compression)
				{
					compression = value;
					OnPropertyChanged("Compression");
				}
			}
		}
		property bool CompressionVisible
		{
			bool get()
			{
				return Script == DNNScripts::densenet;
			}
		}
		property bool SqueezeExcitation
		{
			bool get() { return squeezeExcitation; }
			void set(bool value)
			{
				if (value == squeezeExcitation)
					return;

				squeezeExcitation = value;
				OnPropertyChanged("SqueezeExcitation");
			}
		}
		property bool SqueezeExcitationVisible
		{
			bool get()
			{
				return Script == DNNScripts::mobilenetv3;
			}
		}
		property bool ChannelZeroPad
		{
			bool get() { return channelZeroPad; }
			void set(bool value)
			{
				if (value == channelZeroPad)
					return;

				channelZeroPad = value;
				OnPropertyChanged("ChannelZeroPad");
			}
		}
		property bool ChannelZeroPadVisible
		{
			bool get()
			{
				return Script == DNNScripts::resnet;
			}
		}

		[field:NonSerializedAttribute()]
		virtual event System::ComponentModel::PropertyChangedEventHandler^ PropertyChanged;

		void OnPropertyChanged(String^ propertyName) { PropertyChanged(this, gcnew System::ComponentModel::PropertyChangedEventArgs(propertyName)); }

		DNNModelParameters(DNNScripts model, DNNDatasets dataset, size_t h, size_t w, size_t padH, size_t padW, bool mirrorPad, bool meanStdNorm, DNNFillers weightsFiller, Float weightsScale, Float weightsLRM, Float weightsWDM, bool hasBias, DNNFillers biasesFiller, Float biasesScale, Float biasesLRM, Float biasesWDM, Float batchNormMomentum, Float batchNormEps, bool batchNormScaling, Float alpha, Float beta, size_t groups, size_t iterations, size_t width, size_t growthRate, bool bottleneck, Float dropout, Float compression, bool squeezeExcitation, bool channelZeroPad)
		{
			Script = model;
			Dataset = dataset;
			H = h;
			W = w;
			PadH = padH;
			PadW = padW;
			MirrorPad = mirrorPad;
			MeanStdNormalization = meanStdNorm;

			WeightsFiller = weightsFiller;
			WeightsScale = weightsScale;
			WeightsLRM = weightsLRM;
			WeightsWDM = weightsWDM;
			HasBias = hasBias;
			BiasesFiller = biasesFiller;
			BiasesScale = biasesScale;
			BiasesLRM = biasesLRM;
			BiasesWDM = biasesWDM;

			BatchNormMomentum = batchNormMomentum;
			BatchNormEps = batchNormEps;
			BatchNormScaling = batchNormScaling;

			Alpha = alpha;
			Beta = beta;

			Groups = groups;
			Iterations = iterations;
			Width = width;
			GrowthRate = growthRate;
			Bottleneck = bottleneck;
			Dropout = dropout;
			Compression = compression;
			SqueezeExcitation = squeezeExcitation;
			ChannelZeroPad = channelZeroPad;
		}

	private:
		String^ modelName;
		DNNScripts model;
		DNNDatasets dataset;
		size_t c;
		size_t h;
		size_t w;
		size_t padH;
		size_t padW;
		bool mirrorPad;
		bool meanStdNormalization;
		DNNFillers weightsFiller;
		Float weightsScale = Float(0.05);
		Float weightsLRM = Float(1);
		Float weightsWDM = Float(1);
		bool hasBias = false;
		DNNFillers biasesFiller = DNNFillers::Constant;
		Float biasesScale = Float(0);
		Float biasesLRM = Float(1);
		Float biasesWDM = Float(1);
		Float batchNormMomentum;
		Float batchNormEps;
		bool batchNormScaling;
		Float alpha;
		Float beta;
		size_t groups;
		size_t iterations;
		size_t width;
		size_t growthRate;
		bool bottleneck;
		Float dropout;
		Float compression;
		bool squeezeExcitation;
		bool channelZeroPad;
	};
	*/

	[Serializable()]
	public ref class DNNCostLayer
	{
	public:
		property DNNCosts CostFunction;
		property size_t LayerIndex;
		property size_t GroupIndex;
		property size_t LabelIndex;
		property size_t ClassCount;
		property String^ Name;
		property Float Weight;

		property size_t TrainErrors;
		property Float TrainLoss;
		property Float AvgTrainLoss;
		property Float TrainErrorPercentage;
		property Float TrainAccuracy;

		property size_t TestErrors;
		property Float TestLoss;
		property Float AvgTestLoss;
		property Float TestErrorPercentage;
		property Float TestAccuracy;

		DNNCostLayer(DNNCosts costFunction, size_t layerIndex, size_t groupIndex, size_t labelIndex, size_t classCount, String^ name, Float weight)
		{
			CostFunction = costFunction;
			LayerIndex = layerIndex;
			GroupIndex = groupIndex;
			LabelIndex = labelIndex;
			ClassCount = classCount;
			Name = name;
			Weight = weight;

			TrainErrors = 0;
			TrainLoss = Float(0);
			AvgTrainLoss = Float(0);
			TrainErrorPercentage = Float(0);
			TrainAccuracy = Float(0);

			TestErrors = 0;
			TestLoss = Float(0);
			AvgTestLoss = Float(0);
			TestErrorPercentage = Float(0);
			TestAccuracy = Float(0);
		}
	};

	[Serializable()]
	public ref struct DNNTrainingRate : public System::ComponentModel::INotifyPropertyChanged
	{
	public:
		property size_t BatchSize
		{
			size_t get() { return batchSize; }
			void set(size_t value)
			{
				if (value == batchSize && value == 0)
					return;

				batchSize = value;
				OnPropertyChanged("BatchSize");
			}
		}
		property size_t Cycles
		{
			size_t get() { return cycles; }
			void set(size_t value)
			{
				if (value == cycles && value == 0)
					return;

				cycles = value;
				OnPropertyChanged("Cycles");
			}
		}
		property size_t Epochs
		{
			size_t get() { return epochs; }
			void set(size_t value)
			{
				if (value == epochs && value == 0)
					return;

				epochs = value;
				OnPropertyChanged("Epochs");
			}
		}
		property size_t EpochMultiplier
		{
			size_t get() { return epochMultiplier; }
			void set(size_t value)
			{
				if (value == epochMultiplier && value == 0)
					return;

				epochMultiplier = value;
				OnPropertyChanged("EpochMultiplier");
			}
		}
		property size_t DecayAfterEpochs
		{
			size_t get() { return decayAfterEpochs; }
			void set(size_t value)
			{
				if (value == decayAfterEpochs && value == 0)
					return;

				decayAfterEpochs = value;
				OnPropertyChanged("DecayAfterEpochs");
			}
		}
		property size_t Interpolation
		{
			size_t get() { return interpolation; }
			void set(size_t value)
			{
				if (value == interpolation)
					return;

				interpolation = value;
				OnPropertyChanged("Interpolation");
			}
		}
		property size_t ColorAngle
		{
			size_t get() { return colorAngle; }
			void set(size_t value)
			{
				if (value == colorAngle || value < Float(-360) || value > Float(360))
					return;

				colorAngle = value;
				OnPropertyChanged("ColorAngle");
			}
		}
		property Float ColorCast
		{
			Float get() { return colorCast; }
			void set(Float value)
			{
				if (value == colorCast || value < Float(0) || value > Float(1))
					return;

				colorCast = value;
				OnPropertyChanged("ColorCast");
			}
		}
		property Float Distortion
		{
			Float get() { return distortion; }
			void set(Float value)
			{
				if (value == distortion || value < Float(0) || value > Float(1))
					return;

				distortion = value;
				OnPropertyChanged("Distortion");
			}
		}
		property Float Dropout
		{
			Float get() { return dropout; }
			void set(Float value)
			{
				if (value == dropout || value < Float(0) || value > Float(1))
					return;

				dropout = value;
				OnPropertyChanged("Dropout");
			}
		}
		property Float Cutout
		{
			Float get() { return cutout; }
			void set(Float value)
			{
				if (value == cutout || value < Float(0) || value > Float(1))
					return;

				cutout = value;
				OnPropertyChanged("Cutout");
			}
		}
		property Float AutoAugment
		{
			Float get() { return autoAugment; }
			void set(Float value)
			{
				if (value == autoAugment || value < Float(0) || value > Float(1))
					return;

				autoAugment = value;
				OnPropertyChanged("AutoAugment");
			}
		}
		property Float MaximumRate
		{
			Float get() { return maximumRate; }
			void set(Float value)
			{
				if (value == maximumRate || value < Float(0) || value > Float(1))
					return;

				maximumRate = value;
				OnPropertyChanged("MaximumRate");
			}
		}
		property Float MinimumRate
		{
			Float get() { return minimumRate; }
			void set(Float value)
			{
				if (value == minimumRate || value < Float(0) || value > Float(1))
					return;

				minimumRate = value;
				OnPropertyChanged("MinimumRate");
			}
		}
		property Float L2Penalty
		{
			Float get() { return l2Penalty; }
			void set(Float value)
			{
				if (value == l2Penalty || value < Float(0) || value > Float(1))
					return;

				l2Penalty = value;
				OnPropertyChanged("L2Penalty");
			}
		}
		property Float Momentum
		{
			Float get() { return momentum; }
			void set(Float value)
			{
				if (value == momentum || value < Float(0) || value > Float(1))
					return;

				momentum = value;
				OnPropertyChanged("Momentum");
			}
		}
		property Float DecayFactor
		{
			Float get() { return decayFactor; }
			void set(Float value)
			{
				if (value == decayFactor || value < Float(0) || value > Float(1))
					return;

				decayFactor = value;
				OnPropertyChanged("DecayFactor");
			}
		}
		property Float Scaling
		{
			Float get() { return scaling; }
			void set(Float value)
			{
				if (value == scaling || value <= Float(0) || value > Float(200))
					return;

				scaling = value;
				OnPropertyChanged("Scaling");
			}
		}
		property Float Rotation
		{
			Float get() { return rotation; }
			void set(Float value)
			{
				if (value == rotation || value < Float(0) || value > Float(360))
					return;

				rotation = value;
				OnPropertyChanged("Rotation");
			}
		}
		property bool HorizontalFlip
		{
			bool get() { return horizontalFlip; }
			void set(bool value)
			{
				if (value == horizontalFlip)
					return;

				horizontalFlip = value;
				OnPropertyChanged("HorizontalFlip");
			}
		}
		property bool VerticalFlip
		{
			bool get() { return verticalFlip; }
			void set(bool value)
			{
				if (value == verticalFlip)
					return;

				verticalFlip = value;
				OnPropertyChanged("VerticalFlip");
			}
		}

		DNNTrainingRate(Float maximumRate, size_t batchSize, size_t cycles, size_t epochs, size_t epochMultiplier, Float minRate, Float l2Penalty, Float momentum, Float decayFactor, size_t decayAfterEpochs, bool horizontalFlip, bool verticalFlip, Float dropout, Float cutout, Float autoAugment, Float colorCast, size_t colorAngle, Float distortion, size_t interpolation, Float scaling, Float rotation)
		{
			BatchSize = batchSize;
			Cycles = cycles;
			MaximumRate = maximumRate;
			Epochs = epochs;
			EpochMultiplier = epochMultiplier;
			MinimumRate = minRate;
			L2Penalty = l2Penalty;
			Momentum = momentum;
			DecayFactor = decayFactor;
			DecayAfterEpochs = decayAfterEpochs;
			HorizontalFlip = horizontalFlip;
			VerticalFlip = verticalFlip;
			Dropout = dropout;
			Cutout = cutout;
			AutoAugment = autoAugment;
			ColorCast = colorCast;
			ColorAngle = colorAngle;
			Distortion = distortion;
			Interpolation = interpolation;
			Scaling = scaling;
			Rotation = rotation;
		}

		[field:NonSerializedAttribute()]
		virtual event System::ComponentModel::PropertyChangedEventHandler^ PropertyChanged;

		void OnPropertyChanged(String^ propertyName) { PropertyChanged(this, gcnew System::ComponentModel::PropertyChangedEventArgs(propertyName)); }

	private:
		size_t batchSize = 128;
		size_t cycles = 1;
		size_t epochs = 200;
		size_t epochMultiplier = 1;
		size_t decayAfterEpochs = 1;
		size_t interpolation = 0;
		size_t colorAngle = 0;
		Float colorCast = Float(0);
		Float distortion = Float(0);
		Float dropout = Float(0);
		Float cutout = Float(0);
		Float autoAugment = Float(0);
		Float maximumRate = Float(0.05);
		Float minimumRate = Float(0.0001);
		Float l2Penalty = Float(0.0005);
		Float momentum = Float(0.9);
		Float decayFactor = Float(1);
		Float scaling = Float(10);
		Float rotation = Float(12);
		bool horizontalFlip = false;
		bool verticalFlip = false;
	};

	[Serializable()]
	public ref class DNNTrainingResult
	{
	public:
		property size_t Cycle;
		property size_t Epoch;
		property size_t GroupIndex;
		property size_t CostIndex;
		property String^ CostName;
		property Float Rate;
		property size_t BatchSize;
		property Float Momentum;
		property Float L2Penalty;
		property Float Dropout;
		property Float Cutout;
		property Float AutoAugment;
		property Float ColorCast;
		property size_t ColorAngle;
		property Float Distortion;
		property size_t Interpolation;
		property Float Scaling;
		property Float Rotation;
		property bool HorizontalFlip;
		property bool VerticalFlip;
		property Float AvgTrainLoss;
		property size_t TrainErrors;
		property Float TrainErrorPercentage;
		property Float TrainAccuracy;
		property Float AvgTestLoss;
		property size_t TestErrors;
		property Float TestErrorPercentage;
		property Float TestAccuracy;
		property long long ElapsedTicks;
		property TimeSpan ElapsedTime;

		DNNTrainingResult::DNNTrainingResult() {}

		DNNTrainingResult::DNNTrainingResult(size_t cycle, size_t epoch, size_t groupIndex, size_t costIndex, String^ costName, Float rate, size_t batchSize, Float momentum, Float l2Penalty, Float dropout, Float cutout, Float autoAugment, Float colorCast, size_t colorAngle, Float distortion, size_t interpolation, Float scaling, Float rotation, bool horizontalFlip, bool verticalFlip, Float avgTrainLoss, size_t trainErrors, Float trainErrorPercentage, Float trainAccuracy, Float avgTestLoss, size_t testErrors, Float testErrorPercentage, Float testAccuracy, long long elapsedTicks)
		{
			Cycle = cycle;
			Epoch = epoch;
			GroupIndex = groupIndex;
			CostIndex = costIndex;
			CostName = costName;
			Rate = rate;
			BatchSize = batchSize;
			Momentum = momentum;
			L2Penalty = l2Penalty;
			Dropout = dropout;
			Cutout = cutout;
			AutoAugment = autoAugment;
			ColorCast = colorCast;
			ColorAngle = colorAngle;
			Distortion = distortion;
			Interpolation = interpolation;
			Scaling = scaling;
			Rotation = rotation;
			HorizontalFlip = horizontalFlip;
			VerticalFlip = verticalFlip;
			AvgTrainLoss = avgTrainLoss;
			TrainErrors = trainErrors;
			TrainErrorPercentage = trainErrorPercentage;
			TrainAccuracy = trainAccuracy;
			AvgTestLoss = avgTestLoss;
			TestErrors = testErrors;
			TestErrorPercentage = testErrorPercentage;
			TestAccuracy = testAccuracy;
			ElapsedTicks = elapsedTicks;
			ElapsedTime = TimeSpan(elapsedTicks);
		}
	};

	[Serializable()]
	public ref struct DNNCheckMsg
	{
	public:
		property size_t Row;
		property size_t Column;
		property bool Error;
		property String^ Message;
		property String^ Definition;

		DNNCheckMsg::DNNCheckMsg(size_t row, size_t column, String^ message, bool error, String^ definition)
		{
			Row = row;
			Column = column;
			Message = message;
			Error = error;
			Definition = definition;
		}
	};

	[Serializable()]
	public ref class LayerInformation : public System::ComponentModel::INotifyPropertyChanged
	{
	public:
		property String^ Name;
		property String^ Description;
		property DNNLayerTypes LayerType;
		property DNNActivations ActivationFunctionEnum;
		property DNNCosts CostFunction;
		property System::Collections::Generic::List<size_t>^ Inputs;
		property System::Collections::Generic::List<String^>^ InputsNames;
		property System::Windows::Media::Imaging::BitmapSource^ WeightsSnapshot;
		property bool Lockable;
		property Nullable<bool> LockUpdate
		{
			Nullable<bool> get()
			{
				return lockUpdate;
			}

			void set(Nullable<bool> value)
			{
				if (value.Equals(lockUpdate))
					return;

				lockUpdate = value;
				OnPropertyChanged("LockUpdate");
			}
		};
		property bool HasDropout;
		property bool HasWeights;
		property bool IsNormalizationLayer;
		property bool HasBias;
		property bool MirrorPad;
		property bool RandomCrop;
		property bool Scaling;
		property bool AcrossChannels;
		property int WeightsSnapshotX;
		property int WeightsSnapshotY;
		property size_t InputCount;
		property size_t LayerIndex;
		property size_t NeuronCount;
		property size_t C;
		property size_t D;
		property size_t W;
		property size_t H;
		property size_t HW;
		property size_t KernelH;
		property size_t KernelW;
		property size_t KernelHW;
		property size_t DilationH;
		property size_t DilationW;
		property size_t StrideH;
		property size_t StrideW;
		property size_t PadD;
		property size_t PadH;
		property size_t PadW;
		property size_t Multiplier;
		property size_t Groups;
		property size_t Group;
		property size_t LocalSize;
		property size_t WeightCount;
		property size_t BiasCount;
		property size_t GroupSize;
		property size_t InputC;
		property size_t GroupIndex;
		property size_t LabelIndex;
		property Float Dropout;
		property Float Cutout;
		property Float NeuronsStdDev;
		property Float NeuronsMean;
		property Float NeuronsMin;
		property Float NeuronsMax;
		property Float NeuronsD1StdDev;
		property Float NeuronsD1Mean;
		property Float NeuronsD1Min;
		property Float NeuronsD1Max;
		property Float WeightsStdDev;
		property Float WeightsMean;
		property Float WeightsMin;
		property Float WeightsMax;
		property Float WeightsD1StdDev;
		property Float WeightsD1Mean;
		property Float WeightsD1Min;
		property Float WeightsD1Max;
		property Float BiasesStdDev;
		property Float BiasesMean;
		property Float BiasesMin;
		property Float BiasesMax;
		property Float BiasesD1StdDev;
		property Float BiasesD1Mean;
		property Float BiasesD1Min;
		property Float BiasesD1Max;
		property Float Weight;
		property Float Alpha;
		property Float Beta;
		property Float K;
		property DNNAlgorithms Algorithm;
		property Float FactorH;
		property Float FactorW;
		property Float FPropLayerTime;
		property Float BPropLayerTime;
		property Float UpdateLayerTime;

		[field:NonSerializedAttribute()]
		virtual event System::ComponentModel::PropertyChangedEventHandler^ PropertyChanged;

		void OnPropertyChanged(String^ propertyName) { PropertyChanged(this, gcnew System::ComponentModel::PropertyChangedEventArgs(propertyName)); }

	private:
		Nullable<bool> lockUpdate;
	};

	public ref class Model
	{
	public:
		delegate void TrainProgressEventDelegate(size_t, size_t, size_t, size_t, size_t, bool, bool, Float, Float, Float, Float, size_t, Float, size_t, Float, Float, size_t, Float, Float, Float, Float, Float, Float, size_t, Float, Float, Float, size_t, DNNStates, DNNTaskStates);
		delegate void TestProgressEventDelegate(size_t, size_t, Float, Float, Float, size_t, DNNStates, DNNTaskStates);
		delegate void NewEpochEventDelegate(size_t, size_t, size_t, bool, bool, Float, Float, Float, Float, size_t, Float, size_t, Float, Float, Float, size_t, Float, Float, Float, Float, Float, size_t, Float, Float, Float, size_t);

		property TrainProgressEventDelegate^ TrainProgress;
		property TestProgressEventDelegate^ TestProgress;
		property NewEpochEventDelegate^ NewEpoch;

		property int SelectedIndex;
		property System::Collections::ObjectModel::ObservableCollection<LayerInformation^>^ Layers;
		property System::Windows::Media::Imaging::BitmapSource^ InputSnapshot;
		property String^ Label;
		property cli::array<DNNCostLayer^>^ CostLayers;
		property cli::array<Float>^ MeanTrainSet;
		property cli::array<Float>^ StdTrainSet;
		property cli::array<cli::array<size_t>^>^ ConfusionMatrix;
		property cli::array<cli::array<String^>^>^ LabelsCollection;
		property cli::array<DNNTrainingRate^>^ TrainingRates;
		property DNNTrainingRate^ TrainingRate;
		property String^ DefinitionDocument;
		property String^ StorageDirectory;
		property String^ DatasetsDirectory;
		property String^ DefinitionsDirectory;
		property String^ Name;
		property String^ DurationString;
		property DNNDatasets Dataset;
		property DNNOptimizers Optimizer;
		property DNNCosts CostFunction;
		property Stopwatch^ Duration;
		property bool HorizontalFlip;
		property bool VerticalFlip;
		property bool MeanStdNormalization;
		property bool IsTraining;
		property size_t CostIndex;
		property size_t Hierarchies;
		property size_t ClassCount;
		property size_t GroupIndex;
		property size_t LabelIndex;
		property size_t BatchSize;
		property size_t LayerCount;
		property size_t Multiplier;
		property size_t CostLayersCount;
		property size_t TrainingSamples;
		property size_t AdjustedTrainingSamplesCount;
		property size_t TestingSamples;
		property size_t AdjustedTestingSamplesCount;
		property size_t Cycle;
		property size_t TotalCycles;
		property size_t Epoch;
		property size_t TotalEpochs;
		property Float ColorCast;
		property size_t ColorAngle;
		property size_t Interpolation;
		property size_t SampleIndex;
		property size_t TrainErrors;
		property size_t TestErrors;
		property size_t BlockSize;
		property Float Dropout;
		property Float Cutout;
		property Float AutoAugment;
		property Float Distortion;
		property Float Scaling;
		property Float Rotation;
		property Float AdaDeltaEps;
		property Float AdaGradEps;
		property Float AdamEps;
		property Float AdamBeta2;
		property Float AdamaxEps;
		property Float AdamaxBeta2;
		property Float RMSPropEps;
		property Float RAdamEps;
		property Float RAdamBeta1;
		property Float RAdamBeta2;
		property Float AvgTrainLoss;
		property Float TrainErrorPercentage;
		property Float AvgTestLoss;
		property Float TestErrorPercentage;
		property Float Rate;
		property Float Momentum;
		property Float L2Penalty;
		property Float SampleRate;
		property DNNStates State;
		property DNNTaskStates TaskState;
		property Float fpropTime;
		property Float bpropTime;
		property Float updateTime;
		property bool PersistOptimizer;
		property bool DisableLocking;
		property bool PlainFormat;

		Model(String^ name, String^ fileName, DNNOptimizers optimizer);
		virtual ~Model();

		bool LoadDataset();
		cli::array<String^>^ GetTextLabels(String^ fileName);
		void OnElapsed(Object^ sender, System::Timers::ElapsedEventArgs^ e);
		void UpdateInputSnapshot(size_t C, size_t H, size_t W);
		void SetPersistOptimizer(bool persist);
		void SetDisableLocking(bool disable);
		void SetOptimizersHyperParameters(Float adaDeltaEps, Float adaGradEps, Float adamEps, Float adamBeta2, Float adamaxEps, Float adamaxBeta2, Float rmsPropEps, Float radamEps, Float radamBeta1, Float radamBeta2);
		void ApplyParameters();
		bool SetFormat(bool plain);
		void ResetLayerWeights(size_t layerIndex);
		void ResetWeights();
		void AddLearningRate(bool clear, size_t gotoEpoch, DNNTrainingRate^ rate);
		void AddLearningRateSGDR(bool clear, size_t gotoEpoch, DNNTrainingRate^ rate);
		void Start(bool training);
		void Stop();
		void Pause();
		void Resume();
		void UpdateLayerStatistics(LayerInformation^ info, size_t layerIndex, bool updateUI);
		LayerInformation^ GetLayerInfo(size_t layerIndex, bool updateUI);
		void UpdateLayerInfo(size_t layerIndex, bool updateUI);
		void SetCostIndex(size_t costIndex);
		void SetOptimizer(DNNOptimizers optimizer);
		void ResetOptimizer();
		void SetLocked(bool locked);
		void SetLayerLocked(size_t layerIndex, bool locked);
		void GetConfusionMatrix();
		void UpdateCostInfo(size_t costIndex);
		bool BatchNormalizationUsed();
		DNNCheckMsg^ CheckDefinition(String^ definition);
		int LoadDefinition(String^ fileName);
		int LoadWeights(String^ fileName, bool persistOptimizer);
		int SaveWeights(String^ fileName, bool persistOptimizer);
		int LoadLayerWeights(String^ fileName, size_t layerIndex);
		int SaveLayerWeights(String^ fileName, size_t layerIndex);
		static bool StochasticEnabled();

	protected:
		System::Timers::Timer^ WorkerTimer;
		StringBuilder^ sb;
		String^ oldWeightSaveFileName;
		DNNStates OldState;
	};
}
