#pragma once
#include "Layer.h"

namespace dnn
{
	enum class Activations
	{
		Abs = 0,
		BoundedRelu = 1,
		Clip = 2,
		Elu = 3,
		Exp = 4,
		Gelu = 5,
		GeluErf = 6,
		HardLogistic = 7,
		HardSwish = 8,
		Linear = 9,
		Log = 10,
		Logistic = 11,
		LogLogistic = 12,
		LogSoftmax = 13,
		Mish = 14,
		Pow = 15,
		Relu = 16,
		Round = 17,
		Softmax = 18,
		SoftRelu = 19,
		Sqrt = 20,
		Square = 21,
		Swish = 22,
		Tanh = 23
	};

	class Activation final : public Layer
	{
	public:

		Activation(const dnn::Device& device, const dnnl::memory::format_tag format, const std::string& name, const Activations activation, const std::vector<Layer*>& inputs, const Float alpha = Float(0), const Float beta = Float(0));

		const Activations ActivationFunction;
		const Float Alpha;
		const Float Beta;

		std::string GetDescription() const final override;

		size_t FanIn() const final override;
		size_t FanOut() const final override;

		void InitializeDescriptors(const size_t batchSize) final override;

		void ForwardProp(const size_t batchSize, const bool training) final override;
		void BackwardProp(const size_t batchSize) final override;

	private:
		std::unique_ptr<dnnl::logsoftmax_forward::primitive_desc> fwdDescLogSoftmax;
		std::unique_ptr<dnnl::logsoftmax_backward::primitive_desc> bwdDescLogSoftmax;
		std::unique_ptr<dnnl::softmax_forward::primitive_desc> fwdDescSoftmax;
		std::unique_ptr<dnnl::softmax_backward::primitive_desc> bwdDescSoftmax;
		std::unique_ptr<dnnl::eltwise_forward::primitive_desc> fwdDesc;
		std::unique_ptr<dnnl::eltwise_backward::primitive_desc> bwdDesc;
		std::unique_ptr<dnnl::binary::primitive_desc> bwdAddDesc;

		std::unique_ptr<dnnl::logsoftmax_forward> fwdLogSoftmax;
		std::unique_ptr<dnnl::logsoftmax_backward> bwdLogSoftmax;
		std::unique_ptr<dnnl::softmax_forward> fwdSoftmax;
		std::unique_ptr<dnnl::softmax_backward> bwdSoftmax;
		std::unique_ptr<dnnl::eltwise_forward> fwd;
		std::unique_ptr<dnnl::eltwise_backward> bwd;
		std::unique_ptr<dnnl::binary> bwdAdd;

		dnnl::algorithm algorithm;
		bool reorderFwdSrc;
		bool reorderBwdSrc;
		bool reorderBwdDiffSrc;
	};
}