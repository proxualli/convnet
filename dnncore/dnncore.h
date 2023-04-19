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

namespace dnn
{
	typedef float Float;
	typedef size_t UInt;
	typedef unsigned char Byte;

	enum class TaskStates
	{
		Paused = 0,
		Running = 1,
		Stopped = 2
	};

	enum class States
	{
		Idle = 0,
		NewEpoch = 1,
		Testing = 2,
		Training = 3,
		SaveWeights = 4,
		Completed = 5
	};

	enum class Optimizers
	{
		AdaBound = 0,
		AdaBoundW = 1,
		AdaDelta = 2,
		AdaGrad = 3,
		Adam = 4,
		Adamax = 5,
		AdamW = 6,
		AmsBound = 7,
		AmsBoundW = 8,
		NAG = 9,
		RMSProp = 10,
		SGD = 11,
		SGDMomentum = 12,
		SGDW = 13
	};

	enum class Costs
	{
		BinaryCrossEntropy = 0,
		CategoricalCrossEntropy = 1,
		MeanAbsoluteEpsError = 2,
		MeanAbsoluteError = 3,
		MeanSquaredError = 4,
		SmoothHinge = 5
	};

	enum class Fillers
	{
		Constant = 0,
		HeNormal = 1,
		HeUniform = 2,
		LecunNormal = 3,
		LeCunUniform = 4,
		Normal = 5,
		TruncatedNormal = 6,
		Uniform = 7
	};

	enum class FillerModes
	{
		Avg = 0,
		In = 1,
		Out = 2
	};

	enum class LayerTypes
	{
		Activation = 0,
		Add = 1,
		Average = 2,
		AvgPooling = 3,
		BatchNorm = 4,
		BatchNormActivation = 5,
		BatchNormActivationDropout = 6,
		BatchNormRelu = 7,
		ChannelSplit = 8,
		ChannelZeroPad = 9,
		Concat = 10,
		Convolution = 11,
		ConvolutionTranspose = 12,
		Cost = 13,
		Dense = 14,
		DepthwiseConvolution = 15,
		Divide = 16,
		Dropout = 17,
		GlobalAvgPooling = 18,
		GlobalMaxPooling = 19,
		Input = 20,
		LayerNorm = 21,
		LocalResponseNorm = 22,
		LogSoftmax = 23,
		Max = 24,
		MaxPooling = 25,
		Min = 26,
		Multiply = 27,
		PartialDepthwiseConvolution = 28,
		PRelu = 29,
		Resampling = 30,
		Shuffle = 31,
		Softmax = 32,
		Substract = 33
	};

	enum class Activations
	{
		Abs = 0,
		ASinh = 1,
		BoundedRelu = 2,
		Clip = 3,
		ClipV2 = 4,			//
		Elu = 5,			//
		Exp = 6,			//
		GeluErf = 7,
		GeluTanh = 8,
		HardSigmoid = 9,
		HardSwish = 10,
		Linear = 11,
		Log = 12,
		LogSigmoid = 13,
		Mish = 14,
		Pow = 15,
		Relu = 16,			//
		Round = 17,
		Selu = 18,
		Sigmoid = 19,		//
		SoftPlus = 20,
		SoftRelu = 21,
		SoftSign = 22,
		Sqrt = 23,			//
		Square = 24,
		Swish = 25,
		Tanh = 26,			//
		TanhExp = 27
	};

	enum class Algorithms
	{
		Linear = 0,
		Nearest = 1
	};

	enum class Datasets
	{
		cifar10 = 0,
		cifar100 = 1,
		fashionmnist = 2,
		mnist = 3,
		tinyimagenet = 4
	};

	enum class Models
	{
		densenet = 0,
		efficientnetv2 = 1,
		mobilenetv3 = 2,
		resnet = 3,
		shufflenetv2 = 4
	};

	enum class Positions
	{
		TopLeft = 0,
		TopRight = 1,
		BottomLeft = 2,
		BottomRight = 3,
		Center = 4
	};

	enum class Interpolations
	{
		Cubic = 0,
		Linear = 1,
		Nearest = 2
	};

	struct TrainingRate
	{
		Optimizers Optimizer;
		Float Momentum;
		Float Beta2;
		Float L2Penalty;
		Float Dropout;
		Float Eps;
		UInt N;
		UInt D;
		UInt H;
		UInt W;
		UInt PadD;
		UInt PadH;
		UInt PadW;
		UInt Cycles;
		UInt Epochs;
		UInt EpochMultiplier;
		Float MaximumRate;
		Float MinimumRate;
		Float FinalRate;
		Float Gamma;
		UInt DecayAfterEpochs;
		Float DecayFactor;
		bool HorizontalFlip;
		bool VerticalFlip;
		Float InputDropout;
		Float Cutout;
		bool CutMix;
		Float AutoAugment;
		Float ColorCast;
		UInt ColorAngle;
		Float Distortion;
		Interpolations Interpolation;
		Float Scaling;
		Float Rotation;

		TrainingRate::TrainingRate() :
			Optimizer(Optimizers::NAG),
			Momentum(Float(0.9)),
			Beta2(Float(0.999)),
			L2Penalty(Float(0.0005)),
			Dropout(Float(0)),
			Eps(Float(1E-08)),
			N(1),
			D(1),
			H(32),
			W(32),
			PadD(0),
			PadH(4),
			PadW(4),
			Cycles(1),
			Epochs(200),
			EpochMultiplier(1),
			MaximumRate(Float(0.05)),
			MinimumRate(Float(0.0001)),
			FinalRate(Float(0.1)),
			Gamma(Float(0.003)),
			DecayAfterEpochs(1),
			DecayFactor(Float(1)),
			HorizontalFlip(false),
			VerticalFlip(false),
			InputDropout(Float(0)),
			Cutout(Float(0)),
			CutMix(false),
			AutoAugment(Float(0)),
			ColorCast(Float(0)),
			ColorAngle(0),
			Distortion(Float(0)),
			Interpolation(Interpolations::Cubic),
			Scaling(Float(10.0)),
			Rotation(Float(10.0))
		{
		}

		TrainingRate::TrainingRate(const Optimizers optimizer, const Float momentum, const Float beta2, const Float l2penalty, const Float dropout, const Float eps, const UInt n, const UInt d, const UInt h, const UInt w, const UInt padD, const UInt padH, const UInt padW, const UInt cycles, const UInt epochs, const UInt epochMultiplier, const Float maximumRate, const Float minimumRate, const Float finalRate, const Float gamma, const UInt decayAfterEpochs, const Float decayFactor, const bool horizontalFlip, const bool verticalFlip, const Float inputDropout, const Float cutout, const bool cutMix, const Float autoAugment, const Float colorCast, const UInt colorAngle, const Float distortion, const dnn::Interpolations interpolation, const Float scaling, const Float rotation) :
			Optimizer(optimizer),
			Momentum(momentum),
			Beta2(beta2),
			L2Penalty(l2penalty),
			Dropout(dropout),
			Eps(eps),
			N(n),
			D(d),
			H(h),
			W(w),
			PadD(padD),
			PadH(padH),
			PadW(padW),
			Cycles(cycles),
			Epochs(epochs),
			EpochMultiplier(epochMultiplier),
			MaximumRate(maximumRate),
			MinimumRate(minimumRate),
			FinalRate(finalRate),
			Gamma(gamma),
			DecayAfterEpochs(decayAfterEpochs),
			DecayFactor(decayFactor),
			HorizontalFlip(horizontalFlip),
			VerticalFlip(verticalFlip),
			InputDropout(inputDropout),
			Cutout(cutout),
			CutMix(cutMix),
			AutoAugment(autoAugment),
			ColorCast(colorCast),
			ColorAngle(colorAngle),
			Distortion(distortion),
			Interpolation(interpolation),
			Scaling(scaling),
			Rotation(rotation)
		{
		}
	};

	struct TrainingStrategy
	{
		Float Epochs;
		UInt N;
		UInt D;
		UInt H;
		UInt W;
		UInt PadD;
		UInt PadH;
		UInt PadW;
		Float Momentum;
		Float Beta2;
		Float Gamma;
		Float L2Penalty;
		Float Dropout;
		bool HorizontalFlip;
		bool VerticalFlip;
		Float InputDropout;
		Float Cutout;
		bool CutMix;
		Float AutoAugment;
		Float ColorCast;
		UInt ColorAngle;
		Float Distortion;
		Interpolations Interpolation;
		Float Scaling;
		Float Rotation;

		TrainingStrategy::TrainingStrategy() :
			Epochs(1),
			N(128),
			D(1),
			H(32),
			W(32),
			PadD(0),
			PadH(4),
			PadW(4),
			Momentum(Float(0.9)),
			Beta2(Float(0.999)),
			Gamma(Float(0.003)),
			L2Penalty(Float(0.0005)),
			Dropout(Float(0)),
			HorizontalFlip(false),
			VerticalFlip(false),
			InputDropout(Float(0)),
			Cutout(Float(0)),
			CutMix(false),
			AutoAugment(Float(0)),
			ColorCast(Float(0)),
			ColorAngle(0),
			Distortion(Float(0)),
			Interpolation(Interpolations::Cubic),
			Scaling(Float(10.0)),
			Rotation(Float(10.0))
		{
		}

		TrainingStrategy(const Float epochs, const UInt n, const UInt d, const UInt h, const UInt w, const UInt padD, const UInt padH, const UInt padW, const Float momentum, const Float beta2, const Float gamma, const Float l2penalty, const Float dropout, const bool horizontalFlip, const bool verticalFlip, const Float inputDropout, const Float cutout, const bool cutMix, const Float autoAugment, const Float colorCast, const UInt colorAngle, const Float distortion, const Interpolations interpolation, const Float scaling, const Float rotation) :
			Epochs(epochs),
			N(n),
			D(d),
			H(h),
			W(w),
			PadD(padD),
			PadH(padH),
			PadW(padW),
			Momentum(momentum),
			Beta2(beta2),
			Gamma(gamma),
			L2Penalty(l2penalty),
			Dropout(dropout),
			HorizontalFlip(horizontalFlip),
			VerticalFlip(verticalFlip),
			InputDropout(inputDropout),
			Cutout(cutout),
			CutMix(cutMix),
			AutoAugment(autoAugment),
			ColorCast(colorCast),
			ColorAngle(colorAngle),
			Distortion(distortion),
			Interpolation(interpolation),
			Scaling(scaling),
			Rotation(rotation)
		{
		}
	};

	struct Stats
	{
		Float Mean;
		Float StdDev;
		Float Min;
		Float Max;

		Stats() : Mean(0), StdDev(0), Min(0), Max(0)
		{
		}

		Stats(const Stats& stats) :
			Mean(stats.Mean),
			StdDev(stats.StdDev),
			Min(stats.Min),
			Max(stats.Max)
		{
		}

		Stats(const Float mean, const Float stddev, const Float min, const Float max) :
			Mean(mean),
			StdDev(stddev),
			Min(min),
			Max(max)
		{
		}
	};

	struct CheckMsg
	{
		UInt Row;
		UInt Column;
		bool Error;
		std::string Message;

		CheckMsg(const UInt row = 0, const UInt column = 0, const std::string& message = "", const bool error = true) :
			Row(row),
			Column(column),
			Message(message),
			Error(error)
		{
		}
	};

	struct TrainingInfo
	{
		UInt TotalCycles;
		UInt TotalEpochs;
		UInt Cycle;
		UInt Epoch;
		UInt SampleIndex;
		Float Rate;
		Optimizers Optimizer;
		Float Momentum;
		Float Beta2;
		Float Gamma;
		Float L2Penalty;
		Float Dropout;
		UInt BatchSize;
		UInt Height;
		UInt Width;
		UInt PadH;
		UInt PadW;
		bool HorizontalFlip;
		bool VerticalFlip;
		Float InputDropout;
		Float Cutout;
		bool CutMix;
		Float AutoAugment;
		Float ColorCast;
		UInt ColorAngle;
		Float Distortion;
		Interpolations Interpolation;
		Float Scaling;
		Float Rotation;
		Float AvgTrainLoss;
		Float TrainErrorPercentage;
		UInt TrainErrors;
		Float AvgTestLoss;
		Float TestErrorPercentage;
		UInt TestErrors;
		Float SampleSpeed;
		States State;
		TaskStates TaskState;
	};

	struct TestingInfo
	{
		UInt TotalCycles;
		UInt TotalEpochs;
		UInt Cycle;
		UInt Epoch;
		UInt SampleIndex;
		UInt BatchSize;
		UInt Height;
		UInt Width;
		UInt PadH;
		UInt PadW;
		Float AvgTestLoss;
		Float TestErrorPercentage;
		UInt TestErrors;
		Float SampleSpeed;
		States State;
		TaskStates TaskState;
	};

	struct LayerInfo
	{
		std::string Name;
		std::string Description;
		LayerTypes LayerType;
		Activations Activation;
		Algorithms Algorithm;
		Costs Cost;
		UInt NeuronCount;
		UInt WeightCount;
		UInt BiasesCount;
		UInt LayerIndex;
		UInt InputsCount;
		UInt C;
		UInt D;
		UInt H;
		UInt W;
		UInt PadD;
		UInt PadH;
		UInt PadW;
		UInt KernelH;
		UInt KernelW;
		UInt StrideH;
		UInt StrideW;
		UInt DilationH;
		UInt DilationW;
		UInt Multiplier;
		UInt Groups;
		UInt Group;
		UInt LocalSize;
		Float Dropout;
		Float LabelTrue;
		Float LabelFalse;
		Float Weight;
		UInt GroupIndex;
		UInt LabelIndex;
		UInt InputC;
		Float Alpha;
		Float Beta;
		Float K;
		Float fH;
		Float fW;
		bool HasBias;
		bool Scaling;
		bool AcrossChannels;
		bool Locked;
		bool Lockable;
	};

	struct CostInfo
	{
		UInt TrainErrors;
		Float TrainLoss;
		Float AvgTrainLoss;
		Float TrainErrorPercentage;
		UInt TestErrors;
		Float TestLoss;
		Float AvgTestLoss;
		Float TestErrorPercentage;
	};

	struct ModelInfo
	{
		std::string Name;
		Datasets Dataset;
		Costs CostFunction;
		UInt LayerCount;
		UInt CostLayerCount;
		UInt CostIndex;
		UInt GroupIndex;
		UInt LabelIndex;
		UInt Hierarchies;
		UInt TrainingSamplesCount;
		UInt TestingSamplesCount;
		bool MeanStdNormalization;
		std::vector<Float> MeanTrainSet;
		std::vector<Float> StdTrainSet;
	};

	struct StatsInfo
	{
		std::string Description;
		Stats NeuronsStats;
		Stats WeightsStats;
		Stats BiasesStats;
		Float FPropLayerTime;
		Float BPropLayerTime;
		Float UpdateLayerTime;
		Float FPropTime;
		Float BPropTime;
		Float UpdateTime;
		bool Locked;
	};
}

namespace dnncore
{
	typedef float Float;
	typedef size_t UInt;
	typedef unsigned char Byte;

	constexpr auto FloatSaturate(const Float value) { return value > Float(255) ? Byte(255) : value < Float(0) ? Byte(0) : Byte(value); }

	inline static auto ToUnmanagedString(String^ s) { return msclr::interop::marshal_as<std::string>(s); }

	inline static auto ToManagedString(const std::string& s) { return gcnew String(s.c_str()); }

	[Serializable()]
	public enum class DNNAlgorithms
	{
		Linear = 0,
		Nearest = 1
	};

	[Serializable()]
	public enum class DNNInterpolations
	{
		Cubic = 0,
		Linear = 1,
		Nearest = 2
	};

	[Serializable()]
	public enum class DNNOptimizers
	{
		AdaBound = 0,
		AdaBoundW = 1,
		AdaDelta = 2,
		AdaGrad = 3,
		Adam = 4,
		Adamax = 5,
		AdamW = 6,
		AmsBound = 7,
		AmsBoundW = 8,
		NAG = 9,
		RMSProp = 10,
		SGD = 11,
		SGDMomentum = 12,
		SGDW = 13
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
		efficientnetv2  = 1,
		mobilenetv3 = 2,
		resnet = 3,
		shufflenetv2 = 4
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
		Uniform = 7
	};

	[Serializable()]
	public enum class DNNFillerModes
	{
		Avg = 0,
		In = 1,
		Out = 2
	};

	[Serializable()]
	public enum class DNNLayerTypes
	{
		Activation = 0,
		Add = 1,
		Average = 2,
		AvgPooling = 3,
		BatchNorm = 4,
		BatchNormActivation = 5,
		BatchNormActivationDropout = 6,
		BatchNormRelu = 7,
		ChannelSplit = 8,
		ChannelZeroPad = 9,
		Concat = 10,
		Convolution = 11,
		ConvolutionTranspose = 12,
		Cost = 13,
		Dense = 14,
		DepthwiseConvolution = 15,
		Divide = 16,
		Dropout = 17,
		GlobalAvgPooling = 18,
		GlobalMaxPooling = 19,
		Input = 20,
		LayerNorm = 21,
		LocalResponseNorm = 22,
		LogSoftmax = 23,
		Max = 24,
		MaxPooling = 25,
		Min = 26,
		Multiply = 27,
		PartialDepthwiseConvolution = 28,
		PRelu = 29,
		Resampling = 30,
		Shuffle = 31,
		Softmax = 32,
		Substract = 33
	};

	[Serializable()]
	public enum class DNNActivations
	{
		Abs = 0,
		ASinh = 1,
		BoundedRelu = 2,
		Clip = 3,
		ClipV2 = 4,			//
		Elu = 5,			//
		Exp = 6,			//
		GeluErf = 7,
		GeluTanh = 8,
		HardSigmoid = 9,
		HardSwish = 10,
		Linear = 11,
		Log = 12,
		LogSigmoid = 13,
		Mish = 14,
		Pow = 15,
		Relu = 16,			//
		Round = 17,
		Selu = 18,
		Sigmoid = 19,		//
		SoftPlus = 20,
		SoftRelu = 21,
		SoftSign = 22,
		Sqrt = 23,			//
		Square = 24,
		Swish = 25,
		Tanh = 26,			//
		TanhExp = 27
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

	struct Stats;

	[Serializable()]
	public ref class DNNStats
	{
	public:
		property Float Mean;
		property Float StdDev;
		property Float Min;
		property Float Max;

		DNNStats(const dnn::Stats& stats)
		{
			Mean = stats.Mean;
			StdDev = stats.StdDev;
			Min = stats.Min;
			Max = stats.Max;
		}

		DNNStats::DNNStats(const Float mean, const Float stddev, const Float min, const Float max)
		{
			Mean = mean;
			StdDev = stddev;
			Min = min;
			Max = max;
		}
	};

	[Serializable()]
	public ref class DNNCostLayer
	{
	public:
		property DNNCosts CostFunction;
		property UInt LayerIndex;
		property UInt GroupIndex;
		property UInt LabelIndex;
		property UInt ClassCount;
		property String^ Name;
		property Float Weight;

		property UInt TrainErrors;
		property Float TrainLoss;
		property Float AvgTrainLoss;
		property Float TrainErrorPercentage;
		property Float TrainAccuracy;

		property UInt TestErrors;
		property Float TestLoss;
		property Float AvgTestLoss;
		property Float TestErrorPercentage;
		property Float TestAccuracy;

		DNNCostLayer(DNNCosts costFunction, UInt layerIndex, UInt groupIndex, UInt labelIndex, UInt classCount, String^ name, Float weight)
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
	public ref class DNNTrainingRate : public INotifyPropertyChanged
	{
	public:
		property DNNOptimizers Optimizer
		{
			DNNOptimizers get() { return optimizer; }
			void set(DNNOptimizers value)
			{
				if (value == optimizer)
					return;

				optimizer = value;
				OnPropertyChanged("Optimizer");
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
		property Float Beta2
		{
			Float get() { return beta2; }
			void set(Float value)
			{
				if (value == beta2 || value < Float(0) || value > Float(1))
					return;

				beta2 = value;
				OnPropertyChanged("Beta2");
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
		property Float Eps
		{
			Float get() { return eps; }
			void set(Float value)
			{
				if (value == eps || value < Float(0) || value > Float(1))
					return;

				eps = value;
				OnPropertyChanged("Eps");
			}
		}
		property UInt N
		{
			UInt get() { return n; }
			void set(UInt value)
			{
				if (value == n && value == 0)
					return;

				n = value;
				OnPropertyChanged("N");
			}
		}
		property UInt D
		{
			UInt get() { return d; }
			void set(UInt value)
			{
				if (value == d && value == 0)
					return;

				d = value;
				OnPropertyChanged("D");
			}
		}
		property UInt H
		{
			UInt get() { return h; }
			void set(UInt value)
			{
				if (value == h && value == 0)
					return;

				h = value;
				OnPropertyChanged("H");
			}
		}
		property UInt W
		{
			UInt get() { return w; }
			void set(UInt value)
			{
				if (value == w && value == 0)
					return;

				w = value;
				OnPropertyChanged("W");
			}
		}
		property UInt PadD
		{
			UInt get() { return padD; }
			void set(UInt value)
			{
				if (value == padD && value == 0)
					return;

				padD = value;
				OnPropertyChanged("PadD");
			}
		}
		property UInt PadH
		{
			UInt get() { return padH; }
			void set(UInt value)
			{
				if (value == padH && value == 0)
					return;

				padH = value;
				OnPropertyChanged("PadH");
			}
		}
		property UInt PadW
		{
			UInt get() { return padW; }
			void set(UInt value)
			{
				if (value == padW && value == 0)
					return;

				padW = value;
				OnPropertyChanged("PadW");
			}
		}
		property UInt Cycles
		{
			UInt get() { return cycles; }
			void set(UInt value)
			{
				if (value == cycles && value == 0)
					return;

				cycles = value;
				OnPropertyChanged("Cycles");
			}
		}
		property UInt Epochs
		{
			UInt get() { return epochs; }
			void set(UInt value)
			{
				if (value == epochs && value == 0)
					return;

				epochs = value;
				OnPropertyChanged("Epochs");
			}
		}
		property UInt EpochMultiplier
		{
			UInt get() { return epochMultiplier; }
			void set(UInt value)
			{
				if (value == epochMultiplier && value == 0)
					return;

				epochMultiplier = value;
				OnPropertyChanged("EpochMultiplier");
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
		property Float FinalRate
		{
			Float get() { return finalRate; }
			void set(Float value)
			{
				if (value == finalRate || value < Float(0) || value > Float(1))
					return;

				finalRate = value;
				OnPropertyChanged("FinalRate");
			}
		}
		property Float Gamma
		{
			Float get() { return gamma; }
			void set(Float value)
			{
				if (value == gamma || value < Float(0) || value > Float(1))
					return;

				gamma = value;
				OnPropertyChanged("Gamma");
			}
		}
		property UInt DecayAfterEpochs
		{
			UInt get() { return decayAfterEpochs; }
			void set(UInt value)
			{
				if (value == decayAfterEpochs && value == 0)
					return;

				decayAfterEpochs = value;
				OnPropertyChanged("DecayAfterEpochs");
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
		property Float InputDropout
		{
			Float get() { return inputDropout; }
			void set(Float value)
			{
				if (value == inputDropout || value < Float(0) || value > Float(1))
					return;

				inputDropout = value;
				OnPropertyChanged("InputDropout");
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
		property bool CutMix
		{
			bool get() { return cutMix; }
			void set(bool value)
			{
				if (value == cutMix)
					return;

				cutMix = value;
				OnPropertyChanged("CutMix");
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
		property UInt ColorAngle
		{
			UInt get() { return colorAngle; }
			void set(UInt value)
			{
				if (value == colorAngle || value < Float(-360) || value > Float(360))
					return;

				colorAngle = value;
				OnPropertyChanged("ColorAngle");
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
		property DNNInterpolations Interpolation
		{
			DNNInterpolations get() { return interpolation; }
			void set(DNNInterpolations value)
			{
				if (value == interpolation)
					return;

				interpolation = value;
				OnPropertyChanged("Interpolation");
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

		DNNTrainingRate()
		{
			optimizer = DNNOptimizers::NAG;
			momentum = Float(0.9);
			beta2 = Float(0.999);
			l2Penalty = Float(0.0005);
			dropout = Float(0);
			eps = Float(1E-08);
			n = 128;
			d = 1;
			h = 32;
			w = 32;
			padD = 0;
			padH = 4;
			padW = 4;
			cycles = 1;
			epochs = 200;
			epochMultiplier = 1;
			maximumRate = Float(0.05);
			minimumRate = Float(0.0001);
			finalRate = Float(0.1);
			gamma = Float(0.003);
			decayAfterEpochs = 1;
			decayFactor = Float(1);
			horizontalFlip = true;
			verticalFlip = false;
			inputDropout = Float(0);
			cutout = Float(0);
			cutMix = false;
			autoAugment = Float(0);
			colorCast = Float(0);
			colorAngle = 16;
			distortion = Float(0);
			interpolation = DNNInterpolations::Linear;
			scaling = Float(10);
			rotation = Float(12);
		}

		DNNTrainingRate(DNNOptimizers optimizer, Float momentum, Float beta2, Float l2penalty, Float dropout, Float eps, UInt n, UInt d, UInt h, UInt w, UInt padD, UInt padH, UInt padW, UInt cycles, UInt epochs, UInt epochMultiplier, Float maximumRate, Float minimumRate, Float finalRate, Float gamma, UInt decayAfterEpochs, Float decayFactor, bool horizontalFlip, bool verticalFlip, Float inputDropout, Float cutout, bool cutMix, Float autoAugment, Float colorCast, UInt colorAngle, Float distortion, DNNInterpolations interpolation, Float scaling, Float rotation)
		{
			Optimizer = optimizer;
			Momentum = momentum;
			Beta2 = beta2;
			L2Penalty = l2penalty;
			Dropout = dropout;
			Eps = eps;
			N = n;
			D = d;
			H = h;
			W = w;
			PadD = padD;
			PadH = padH;
			PadW = padW;
			Cycles = cycles;
			Epochs = epochs;
			EpochMultiplier = epochMultiplier;
			MaximumRate = maximumRate;
			MinimumRate = minimumRate;
			FinalRate = finalRate;
			Gamma = gamma;
			DecayAfterEpochs = decayAfterEpochs;
			DecayFactor = decayFactor;
			HorizontalFlip = horizontalFlip;
			VerticalFlip = verticalFlip;
			InputDropout = inputDropout;
			Cutout = cutout;
			CutMix = cutMix;
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
			DNNOptimizers optimizer = DNNOptimizers::NAG;
			Float momentum = Float(0.9);
			Float beta2 = Float(0.999);
			Float l2Penalty = Float(0.0005);
			Float dropout = Float(0);
			Float eps = Float(1E-08);
			UInt n = 128;
			UInt d = 1;
			UInt h = 32;
			UInt w = 32;
			UInt padD = 0;
			UInt padH = 4;
			UInt padW = 4;
			UInt cycles = 1;
			UInt epochs = 200;
			UInt epochMultiplier = 1;
			Float maximumRate = Float(0.05);
			Float minimumRate = Float(0.0001);
			Float finalRate = Float(0.1);
			Float gamma = Float(0.003);
			UInt decayAfterEpochs = 1;
			Float decayFactor = Float(1);
			bool horizontalFlip = false;
			bool verticalFlip = false;
			Float inputDropout = Float(0);
			Float cutout = Float(0);
			bool cutMix = false;
			Float autoAugment = Float(0);
			Float colorCast = Float(0);
			UInt colorAngle = 0;
			Float distortion = Float(0);
			DNNInterpolations interpolation = DNNInterpolations::Linear;
			Float scaling = Float(10);
			Float rotation = Float(12);
	};

	[Serializable()]
	public ref class DNNTrainingStrategy : public INotifyPropertyChanged
	{
	public:
		property Float Epochs
		{
			Float get() { return epochs; }
			void set(Float value)
			{
				if (value == epochs || value <= 0 || value > 1)
					return;

				epochs = value;
				OnPropertyChanged("Epochs");
			}
		}
		property UInt N
		{
			UInt get() { return n; }
			void set(UInt value)
			{
				if (value == n && value == 0)
					return;

				n = value;
				OnPropertyChanged("N");
			}
		}
		property UInt D
		{
			UInt get() { return d; }
			void set(UInt value)
			{
				if (value == d && value == 0)
					return;

				d = value;
				OnPropertyChanged("D");
			}
		}
		property UInt H
		{
			UInt get() { return h; }
			void set(UInt value)
			{
				if (value == h && value == 0)
					return;

				h = value;
				OnPropertyChanged("H");
			}
		}
		property UInt W
		{
			UInt get() { return w; }
			void set(UInt value)
			{
				if (value == w && value == 0)
					return;

				w = value;
				OnPropertyChanged("W");
			}
		}
		property UInt PadD
		{
			UInt get() { return padD; }
			void set(UInt value)
			{
				if (value == padD && value == 0)
					return;

				padD = value;
				OnPropertyChanged("PadD");
			}
		}
		property UInt PadH
		{
			UInt get() { return padH; }
			void set(UInt value)
			{
				if (value == padH && value == 0)
					return;

				padH = value;
				OnPropertyChanged("PadH");
			}
		}
		property UInt PadW
		{
			UInt get() { return padW; }
			void set(UInt value)
			{
				if (value == padW && value == 0)
					return;

				padW = value;
				OnPropertyChanged("PadW");
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
		property Float Beta2
		{
			Float get() { return beta2; }
			void set(Float value)
			{
				if (value == beta2 || value < Float(0) || value > Float(1))
					return;

				beta2 = value;
				OnPropertyChanged("Beta2");
			}
		}
		property Float Gamma
		{
			Float get() { return gamma; }
			void set(Float value)
			{
				if (value == gamma || value < Float(0) || value > Float(1))
					return;

				gamma = value;
				OnPropertyChanged("Gamma");
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
		property Float InputDropout
		{
			Float get() { return inputDropout; }
			void set(Float value)
			{
				if (value == inputDropout || value < Float(0) || value > Float(1))
					return;

				inputDropout = value;
				OnPropertyChanged("InputDropout");
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
		property bool CutMix
		{
			bool get() { return cutMix; }
			void set(bool value)
			{
				if (value == cutMix)
					return;

				cutMix = value;
				OnPropertyChanged("CutMix");
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
		property UInt ColorAngle
		{
			UInt get() { return colorAngle; }
			void set(UInt value)
			{
				if (value == colorAngle || value < Float(-360) || value > Float(360))
					return;

				colorAngle = value;
				OnPropertyChanged("ColorAngle");
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
		property DNNInterpolations Interpolation
		{
			DNNInterpolations get() { return interpolation; }
			void set(DNNInterpolations value)
			{
				if (value == interpolation)
					return;

				interpolation = value;
				OnPropertyChanged("Interpolation");
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

		DNNTrainingStrategy()
		{
			epochs = Float(1);
			n = 128;
			d = 1;
			h = 32;
			w = 32;
			padD = 0;
			padH = 4;
			padW = 4;
			momentum = Float(0.9);
			beta2 = Float(0.999);
			gamma = Float(0.003);
			l2Penalty = Float(0.0005);
			dropout = Float(0);
			horizontalFlip = true;
			verticalFlip = false;
			inputDropout = Float(0);
			cutout = Float(0);
			cutMix = false;
			autoAugment = Float(0);
			colorCast = Float(0);
			colorAngle = 16;
			distortion = Float(0);
			interpolation = DNNInterpolations::Linear;
			scaling = Float(10);
			rotation = Float(12);
		}

		DNNTrainingStrategy(Float epochs, UInt n, UInt d, UInt h, UInt w, UInt padD, UInt padH, UInt padW, Float momentum, Float beta2, Float gamma, Float l2penalty, Float dropout, bool horizontalFlip, bool verticalFlip, Float inputDropout, Float cutout, bool cutMix, Float autoAugment, Float colorCast, UInt colorAngle, Float distortion, DNNInterpolations interpolation, Float scaling, Float rotation)
		{
			Epochs = epochs;
			N = n;
			D = d;
			H = h;
			W = w;
			PadD = padD;
			PadH = padH;
			PadW = padW;
			Momentum = momentum;
			Beta2 = beta2;
			Gamma = gamma;
			L2Penalty = l2penalty;
			Dropout = dropout;
			HorizontalFlip = horizontalFlip;
			VerticalFlip = verticalFlip;
			InputDropout = inputDropout;
			Cutout = cutout;
			CutMix = cutMix;
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
		Float epochs = Float(1);
		UInt n = 128;
		UInt d = 1;
		UInt h = 32;
		UInt w = 32;
		UInt padD = 1;
		UInt padH = 4;
		UInt padW = 4;
		Float momentum = Float(0.9);
		Float beta2 = Float(0.999);
		Float gamma = Float(0.003);
		Float l2Penalty = Float(0.0005);
		Float dropout = Float(0);
		bool horizontalFlip = false;
		bool verticalFlip = false;
		Float inputDropout = Float(0);
		Float cutout = Float(0);
		bool cutMix = false;
		Float autoAugment = Float(0);
		Float colorCast = Float(0);
		UInt colorAngle = 0;
		Float distortion = Float(0);
		DNNInterpolations interpolation = DNNInterpolations::Linear;
		Float scaling = Float(10);
		Float rotation = Float(12);
	};

	[Serializable()]
	public ref class DNNTrainingResult
	{
	public:
		property UInt Cycle;
		property UInt Epoch;
		property UInt GroupIndex;
		property UInt CostIndex;
		property String^ CostName;
		property DNNOptimizers Optimizer;
		property Float Momentum;
		property Float Beta2;
		property Float Gamma;
		property Float L2Penalty;
		property Float Dropout;
		property Float Eps;
		property Float Rate;
		property UInt N;
		property UInt D;
		property UInt H;
		property UInt W;
		property UInt PadD;
		property UInt PadH;
		property UInt PadW;
		property Float InputDropout;
		property Float Cutout;
		property bool CutMix;
		property Float AutoAugment;
		property bool HorizontalFlip;
		property bool VerticalFlip;
		property Float ColorCast;
		property UInt ColorAngle;
		property Float Distortion;
		property DNNInterpolations Interpolation;
		property Float Scaling;
		property Float Rotation;
		property Float AvgTrainLoss;
		property UInt TrainErrors;
		property Float TrainErrorPercentage;
		property Float TrainAccuracy;
		property Float AvgTestLoss;
		property UInt TestErrors;
		property Float TestErrorPercentage;
		property Float TestAccuracy;
		property long long ElapsedTicks;
		property TimeSpan ElapsedTime;

		DNNTrainingResult::DNNTrainingResult()
		{
		}

		DNNTrainingResult::DNNTrainingResult(UInt cycle, UInt epoch, UInt groupIndex, UInt costIndex, String^ costName, DNNOptimizers optimizer, Float momentum, Float beta2, Float gamma, Float l2Penalty, Float dropout, Float eps, Float rate, UInt n, UInt d, UInt h, UInt w, UInt padD, UInt padH, UInt padW, Float inputDropout, Float cutout, bool cutMix, Float autoAugment, bool horizontalFlip, bool verticalFlip, Float colorCast, UInt colorAngle, Float distortion, DNNInterpolations interpolation, Float scaling, Float rotation, Float avgTrainLoss, UInt trainErrors, Float trainErrorPercentage, Float trainAccuracy, Float avgTestLoss, UInt testErrors, Float testErrorPercentage, Float testAccuracy, long long elapsedTicks)
		{
			Cycle = cycle;
			Epoch = epoch;
			GroupIndex = groupIndex;
			CostIndex = costIndex;
			CostName = costName;
			Optimizer = optimizer;
			Momentum = momentum;
			Beta2 = beta2;
			Gamma = gamma;
			L2Penalty = l2Penalty;
			Dropout = dropout;
			Eps = eps;
			Rate = rate;
			N = n;
			D = d;
			H = h;
			W = w;
			PadD = padD;
			PadH = padH;
			PadW = padW;
			InputDropout = inputDropout;
			Cutout = cutout;
			CutMix = cutMix;
			AutoAugment = autoAugment;
			HorizontalFlip = horizontalFlip;
			VerticalFlip = verticalFlip;
			ColorCast = colorCast;
			ColorAngle = colorAngle;
			Distortion = distortion;
			Interpolation = interpolation;
			Scaling = scaling;
			Rotation = rotation;
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
		property UInt Row;
		property UInt Column;
		property bool Error;
		property String^ Message;
		property String^ Definition;

		DNNCheckMsg::DNNCheckMsg(UInt row, UInt column, String^ message, bool error, String^ definition)
		{
			Row = row;
			Column = column;
			Message = message;
			Error = error;
			Definition = definition;
		}
	};

	[Serializable()]
	public ref class DNNLayerInfo : public System::ComponentModel::INotifyPropertyChanged
	{
	public:
		property String^ Name;
		property String^ Description;
		property DNNLayerTypes LayerType;
		property DNNActivations Activation;
		property DNNCosts Cost;
		property System::Collections::Generic::List<UInt>^ Inputs;
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
		property bool IsNormalizationLayer;
		property bool HasBias;
		property bool MirrorPad;
		property bool RandomCrop;
		property bool Scaling;
		property bool AcrossChannels;
		property int WeightsSnapshotX;
		property int WeightsSnapshotY;
		property UInt InputCount;
		property UInt LayerIndex;
		property UInt NeuronCount;
		property UInt C;
		property UInt D;
		property UInt W;
		property UInt H;
		property UInt KernelH;
		property UInt KernelW;
		property UInt DilationH;
		property UInt DilationW;
		property UInt StrideH;
		property UInt StrideW;
		property UInt PadD;
		property UInt PadH;
		property UInt PadW;
		property UInt Multiplier;
		property UInt Groups;
		property UInt Group;
		property UInt LocalSize;
		property UInt WeightCount;
		property UInt BiasCount;
		property UInt GroupSize;
		property UInt InputC;
		property UInt GroupIndex;
		property UInt LabelIndex;
		property Float Dropout;
		property Float Cutout;
		property DNNStats^ NeuronsStats;
		property DNNStats^ WeightsStats;
		property DNNStats^ BiasesStats;
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

		DNNLayerInfo()
		{
			NeuronsStats = gcnew DNNStats(0.0f, 0.0f, 0.0f, 0.0f);
			WeightsStats = gcnew DNNStats(0.0f, 0.0f, 0.0f, 0.0f);
			BiasesStats = gcnew DNNStats(0.0f, 0.0f, 0.0f, 0.0f);
		}

		void OnPropertyChanged(String^ propertyName) { PropertyChanged(this, gcnew System::ComponentModel::PropertyChangedEventArgs(propertyName)); }

	private:
		Nullable<bool> lockUpdate;
	};

	public ref class DNNModel
	{
	public:
		delegate void TrainProgressEventDelegate(DNNOptimizers, UInt, UInt, UInt, UInt, UInt, bool, bool, Float, Float, bool, Float, Float, UInt, Float, DNNInterpolations, Float, Float, UInt, Float, Float, Float, Float, Float, Float, Float, Float, Float, UInt, Float, Float, Float, UInt, DNNStates, DNNTaskStates);
		delegate void TestProgressEventDelegate(UInt, UInt, Float, Float, Float, UInt, DNNStates, DNNTaskStates);
		delegate void NewEpochEventDelegate(UInt, UInt, UInt, UInt, Float, Float, Float, bool, bool, Float, Float, bool, Float, Float, UInt, Float, UInt, Float, Float, Float, UInt, UInt, UInt, UInt, UInt, UInt, UInt, Float, Float, Float, Float, Float, Float, UInt, Float, Float, Float, UInt);

		property TrainProgressEventDelegate^ TrainProgress;
		property TestProgressEventDelegate^ TestProgress;
		property NewEpochEventDelegate^ NewEpoch;

		property Byte BackgroundColor;
		property int SelectedIndex;
		property System::Collections::ObjectModel::ObservableCollection<DNNLayerInfo^>^ Layers;
		property System::Windows::Media::Imaging::BitmapSource^ InputSnapshot;
		property String^ Label;
		property cli::array<DNNCostLayer^>^ CostLayers;
		property cli::array<Float>^ MeanTrainSet;
		property cli::array<Float>^ StdTrainSet;
		property cli::array<cli::array<UInt>^>^ ConfusionMatrix;
		property cli::array<cli::array<String^>^>^ LabelsCollection;
		property bool UseTrainingStrategy;
		property System::Collections::ObjectModel::ObservableCollection<DNNTrainingStrategy^>^ TrainingStrategies;
		property cli::array<DNNTrainingRate^>^ TrainingRates;
		property DNNTrainingRate^ TrainingRate;
		property String^ Definition;
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
		property UInt CostIndex;
		property UInt Hierarchies;
		property UInt ClassCount;
		property UInt GroupIndex;
		property UInt LabelIndex;
		property UInt BatchSize;
		property UInt Height;
		property UInt Width;
		property UInt PadH;
		property UInt PadW;
		property UInt LayerCount;
		property UInt Multiplier;
		property UInt CostLayerCount;
		property UInt TrainingSamples;
		property UInt AdjustedTrainingSamplesCount;
		property UInt TestingSamples;
		property UInt AdjustedTestingSamplesCount;
		property UInt Cycle;
		property UInt TotalCycles;
		property UInt Epoch;
		property UInt TotalEpochs;
		property Float Gamma;
		property Float ColorCast;
		property UInt ColorAngle;
		property DNNInterpolations Interpolation;
		property UInt SampleIndex;
		property UInt TrainErrors;
		property UInt TestErrors;
		property UInt BlockSize;
		property Float InputDropout;
		property Float Cutout;
		property bool CutMix;
		property Float AutoAugment;
		property Float Distortion;
		property Float Scaling;
		property Float Rotation;
		property Float AvgTrainLoss;
		property Float TrainErrorPercentage;
		property Float AvgTestLoss;
		property Float TestErrorPercentage;
		property Float Rate;
		property Float Momentum;
		property Float Beta2;
		property Float L2Penalty;
		property Float Dropout;
		property Float SampleRate;
		property DNNStates State;
		property DNNStates OldState;
		property DNNTaskStates TaskState;
		property Float fpropTime;
		property Float bpropTime;
		property Float updateTime;
		property bool PersistOptimizer;
		property bool DisableLocking;
		property bool PlainFormat;

		DNNModel(String^ definition);
		virtual ~DNNModel();
			
		bool LoadDataset();
		bool SetShuffleCount(UInt count);
		cli::array<String^>^ GetTextLabels(String^ fileName);
		void OnElapsed(Object^ sender, System::Timers::ElapsedEventArgs^ e);
		void SetPersistOptimizer(bool persist);
		void SetUseTrainingStrategy(bool enable);
		void SetDisableLocking(bool disable);
		void ApplyParameters();
		bool SetFormat(bool plain);
		void ResetLayerWeights(UInt layerIndex);
		void ResetWeights();
		void AddTrainingRate(DNNTrainingRate^ rate, bool clear, UInt gotoEpoch, UInt trainSamples);
		void AddTrainingRateSGDR(DNNTrainingRate^ rate, bool clear, UInt gotoEpoch, UInt trainSamples);
		void ClearTrainingStrategies();
		void AddTrainingStrategy(DNNTrainingStrategy^ strategy);
		void Start(bool training);
		void Stop();
		void Pause();
		void Resume();
		DNNLayerInfo^ GetLayerInfo(DNNLayerInfo^ infoManaged, UInt layerIndex);
		void UpdateLayerStatistics(DNNLayerInfo^ info, UInt layerIndex, bool updateUI);
		void UpdateLayerInfo(UInt layerIndex, bool updateUI);
		void SetCostIndex(UInt costIndex);
		void SetOptimizer(DNNOptimizers optimizer);
		void ResetOptimizer();
		void SetLocked(bool locked);
		void SetLayerLocked(UInt layerIndex, bool locked);
		void GetConfusionMatrix();
		void UpdateCostInfo(UInt costIndex);
		bool BatchNormUsed();
		DNNCheckMsg^ Check(String^ definition);
		int Load(String^ fileName);
		bool LoadModel(String^ fileName);
		bool SaveModel(String^ fileName);
		bool ClearLog();
		bool LoadLog(String^ fileName);
		bool SaveLog(String^ fileName);
		int LoadWeights(String^ fileName, bool persistOptimizer);
		int SaveWeights(String^ fileName, bool persistOptimizer);
		int LoadLayerWeights(String^ fileName, UInt layerIndex);
		int SaveLayerWeights(String^ fileName, UInt layerIndex);
		static bool StochasticEnabled();

	protected:
		System::Timers::Timer^ WorkerTimer;
		StringBuilder^ sb;
		//String^ oldWeightSaveFileName;
	};
}