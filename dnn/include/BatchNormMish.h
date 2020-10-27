#pragma once
#include "Layer.h"

namespace dnn
{
	class BatchNormMish final : public Layer
	{
	public:
		BatchNormMish(const dnn::Device& device, const dnnl::memory::format_tag format, const std::string& name, const std::vector<Layer*>& inputs, const bool scaling = true, const Float momentum = Float(0.99), const Float eps = Float(1e-04), const bool hasBias = true);
				
		const bool Scaling;
		const Float Eps;
		const Float Momentum;
		const Float OneMinusMomentum;

		FloatVector Mean;
		FloatVector RunningMean;
		FloatVector Variance;
		FloatVector RunningVariance;
		FloatVector InvStdDev;
		FloatVector SrcDiff;

		std::string GetDescription() const final override;

		size_t FanIn() const final override;
		size_t FanOut() const final override;

		void InitializeDescriptors(const size_t batchSize) final override;

		bool Lockable() const final override
		{
			return WeightCount > 0 && Scaling;
		}

		void SetBatchSize(const size_t batchSize) override;

		void ForwardProp(const size_t batchSize, const bool training) final override;
		void BackwardProp(const size_t batchSize) final override;

		ByteVector GetImage(const Byte fillColor) final override;

		void ResetWeights(const Fillers weightFiller, const Float weightFillerScale, const Fillers biasFiller, const Float biasFillerScale) override
		{
			Weights = FloatVector(PaddedC, Float(1));
			Biases = FloatVector(PaddedC, Float(0));

			RunningMean = FloatVector(PaddedC, Float(0));
			RunningVariance = FloatVector(PaddedC, Float(1));
		}

		void Save(std::ostream& os, const bool persistOptimizer = false, const Optimizers optimizer = Optimizers::SGD) override
		{
			os.write(reinterpret_cast<const char*>(RunningMean.data()), std::streamsize(C * sizeof(Float)));
			os.write(reinterpret_cast<const char*>(RunningVariance.data()), std::streamsize(C * sizeof(Float)));

			Layer::Save(os, persistOptimizer, optimizer);
		}

		void Load(std::istream& is, const bool persistOptimizer = false, const Optimizers optimizer = Optimizers::SGD) override
		{
			is.read(reinterpret_cast<char*>(RunningMean.data()), std::streamsize(C * sizeof(Float)));
			is.read(reinterpret_cast<char*>(RunningVariance.data()), std::streamsize(C * sizeof(Float)));

			Layer::Load(is, persistOptimizer, optimizer);
		}

		size_t GetWeightsSize(const bool persistOptimizer = false, const Optimizers optimizer = Optimizers::SGD) const override
		{
			return (2 * C * sizeof(Float)) + Layer::GetWeightsSize(persistOptimizer, optimizer);
		}

		size_t GetNeuronsSize(const size_t batchSize) const override
		{
			size_t totalSize = 0;

#ifndef DNN_LEAN
			totalSize += PaddedCDHW * batchSize * sizeof(Float) * 3;
#else
			totalSize += PaddedCDHW * batchSize * sizeof(Float) * 2;
#endif // DNN_LEAN

			return totalSize;
		}

	private:
		bool plainFormat;
	};
}
