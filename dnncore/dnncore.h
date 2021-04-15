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
	typedef size_t UInt;
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
		BatchNormSwishDropout = 13,
		BatchNormTanhExp = 14,
		BatchNormTanhExpDropout = 15,
		ChannelMultiply = 16,
		ChannelShuffle = 17,
		ChannelSplit = 18,
		ChannelZeroPad = 19,
		Concat = 20,
		Convolution = 21,
		ConvolutionTranspose = 22,
		Cost = 23,
		Dense = 24,
		DepthwiseConvolution = 25,
		Divide = 26,
		Dropout = 27,
		GlobalAvgPooling = 28,
		GlobalMaxPooling = 29,
		Input = 30,
		LocalResponseNormalization = 31,
		Max = 32,
		MaxPooling = 33,
		Min = 34,
		Multiply = 35,
		PartialDepthwiseConvolution = 36,
		Resampling = 37,
		Substract = 38
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
		Tanh = 26,
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
	public ref struct DNNTrainingRate : public System::ComponentModel::INotifyPropertyChanged
	{
	public:
		property System::Array^ OptimizerList
		{
			System::Array^ get() { return Enum::GetValues(DNNOptimizers::typeid); }
		}
		property System::Array^ InterpolationList
		{
			System::Array^ get() { return Enum::GetValues(DNNInterpolation::typeid); }
		}

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
		property Float Beta1
		{
			Float get() { return beta1; }
			void set(Float value)
			{
				if (value == beta1 || value < Float(0) || value > Float(1))
					return;

				beta1 = value;
				OnPropertyChanged("Beta1");
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
		property UInt BatchSize
		{
			UInt get() { return batchSize; }
			void set(UInt value)
			{
				if (value == batchSize && value == 0)
					return;

				batchSize = value;
				OnPropertyChanged("BatchSize");
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
		property DNNInterpolation Interpolation
		{
			DNNInterpolation get() { return interpolation; }
			void set(DNNInterpolation value)
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
		
		DNNTrainingRate(DNNOptimizers optimizer, Float momentum, Float l2penalty, Float beta1, Float beta2, Float eps, UInt batchSize, UInt cycles, UInt epochs, UInt epochMultiplier, Float maximumRate, Float minimumRate, Float decayFactor, UInt decayAfterEpochs, bool horizontalFlip, bool verticalFlip, Float dropout, Float cutout, Float autoAugment, Float colorCast, UInt colorAngle, Float distortion, DNNInterpolation interpolation, Float scaling, Float rotation)
		{
			Optimizer = optimizer;
			Momentum = momentum;
			L2Penalty = l2penalty;
			Beta1 = beta1;
			Beta2 = beta2;
			Eps = eps;
			BatchSize = batchSize;
			Cycles = cycles;
			Epochs = epochs;
			EpochMultiplier = epochMultiplier;
			MaximumRate = maximumRate;
			MinimumRate = minimumRate;
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
		DNNOptimizers optimizer = DNNOptimizers::NAG;
		Float momentum = Float(0.9);
		Float l2Penalty = Float(0.0005);
		Float beta1 = Float(0.9);
		Float beta2 = Float(0.999);
		Float eps = Float(0.00001);
		UInt batchSize = 128;
		UInt cycles = 1;
		UInt epochs = 200;
		UInt epochMultiplier = 1;
		Float maximumRate = Float(0.05);
		Float minimumRate = Float(0.0001);
		UInt decayAfterEpochs = 1;
		Float decayFactor = Float(1);
		bool horizontalFlip = false;
		bool verticalFlip = false;
		Float dropout = Float(0);
		Float cutout = Float(0);
		Float autoAugment = Float(0);
		Float colorCast = Float(0);
		UInt colorAngle = 0;
		Float distortion = Float(0);
		DNNInterpolation interpolation = DNNInterpolation::Cubic;
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
		property Float Rate;
		property UInt BatchSize;
		property Float Momentum;
		property Float L2Penalty;
		property Float Dropout;
		property Float Cutout;
		property Float AutoAugment;
		property Float ColorCast;
		property UInt ColorAngle;
		property Float Distortion;
		property UInt Interpolation;
		property Float Scaling;
		property Float Rotation;
		property bool HorizontalFlip;
		property bool VerticalFlip;
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

		DNNTrainingResult::DNNTrainingResult() {}

		DNNTrainingResult::DNNTrainingResult(UInt cycle, UInt epoch, UInt groupIndex, UInt costIndex, String^ costName, Float rate, UInt batchSize, Float momentum, Float l2Penalty, Float dropout, Float cutout, Float autoAugment, Float colorCast, UInt colorAngle, Float distortion, UInt interpolation, Float scaling, Float rotation, bool horizontalFlip, bool verticalFlip, Float avgTrainLoss, UInt trainErrors, Float trainErrorPercentage, Float trainAccuracy, Float avgTestLoss, UInt testErrors, Float testErrorPercentage, Float testAccuracy, long long elapsedTicks)
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
	public ref class LayerInformation : public System::ComponentModel::INotifyPropertyChanged
	{
	public:
		property String^ Name;
		property String^ Description;
		property DNNLayerTypes LayerType;
		property DNNActivations ActivationFunctionEnum;
		property DNNCosts CostFunction;
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
		property UInt InputCount;
		property UInt LayerIndex;
		property UInt NeuronCount;
		property UInt C;
		property UInt D;
		property UInt W;
		property UInt H;
		property UInt HW;
		property UInt KernelH;
		property UInt KernelW;
		property UInt KernelHW;
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
		delegate void TrainProgressEventDelegate(UInt, UInt, UInt, UInt, UInt, bool, bool, Float, Float, Float, Float, UInt, Float, DNNInterpolation, Float, Float, UInt, Float, Float, Float, Float, Float, Float, UInt, Float, Float, Float, UInt, DNNStates, DNNTaskStates);
		delegate void TestProgressEventDelegate(UInt, UInt, Float, Float, Float, UInt, DNNStates, DNNTaskStates);
		delegate void NewEpochEventDelegate(UInt, UInt, UInt, bool, bool, Float, Float, Float, Float, UInt, Float, UInt, Float, Float, Float, UInt, Float, Float, Float, Float, Float, UInt, Float, Float, Float, UInt);

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
		property cli::array<cli::array<UInt>^>^ ConfusionMatrix;
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
		property UInt CostIndex;
		property UInt Hierarchies;
		property UInt ClassCount;
		property UInt GroupIndex;
		property UInt LabelIndex;
		property UInt BatchSize;
		property UInt LayerCount;
		property UInt Multiplier;
		property UInt CostLayersCount;
		property UInt TrainingSamples;
		property UInt AdjustedTrainingSamplesCount;
		property UInt TestingSamples;
		property UInt AdjustedTestingSamplesCount;
		property UInt Cycle;
		property UInt TotalCycles;
		property UInt Epoch;
		property UInt TotalEpochs;
		property Float ColorCast;
		property UInt ColorAngle;
		property DNNInterpolation Interpolation;
		property UInt SampleIndex;
		property UInt TrainErrors;
		property UInt TestErrors;
		property UInt BlockSize;
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
		void UpdateInputSnapshot(UInt C, UInt H, UInt W);
		void SetPersistOptimizer(bool persist);
		void SetDisableLocking(bool disable);
		void SetOptimizersHyperParameters(Float adaDeltaEps, Float adaGradEps, Float adamEps, Float adamBeta2, Float adamaxEps, Float adamaxBeta2, Float rmsPropEps, Float radamEps, Float radamBeta1, Float radamBeta2);
		void ApplyParameters();
		bool SetFormat(bool plain);
		void ResetLayerWeights(UInt layerIndex);
		void ResetWeights();
		void AddLearningRate(bool clear, UInt gotoEpoch, DNNTrainingRate^ rate);
		void AddLearningRateSGDR(bool clear, UInt gotoEpoch, DNNTrainingRate^ rate);
		void Start(bool training);
		void Stop();
		void Pause();
		void Resume();
		void UpdateLayerStatistics(LayerInformation^ info, UInt layerIndex, bool updateUI);
		LayerInformation^ GetLayerInfo(UInt layerIndex, bool updateUI);
		void UpdateLayerInfo(UInt layerIndex, bool updateUI);
		void SetCostIndex(UInt costIndex);
		void SetOptimizer(DNNOptimizers optimizer);
		void ResetOptimizer();
		void SetLocked(bool locked);
		void SetLayerLocked(UInt layerIndex, bool locked);
		void GetConfusionMatrix();
		void UpdateCostInfo(UInt costIndex);
		bool BatchNormalizationUsed();
		DNNCheckMsg^ CheckDefinition(String^ definition);
		int LoadDefinition(String^ fileName);
		int LoadWeights(String^ fileName, bool persistOptimizer);
		int SaveWeights(String^ fileName, bool persistOptimizer);
		int LoadLayerWeights(String^ fileName, UInt layerIndex);
		int SaveLayerWeights(String^ fileName, UInt layerIndex);
		static bool StochasticEnabled();

	protected:
		System::Timers::Timer^ WorkerTimer;
		StringBuilder^ sb;
		String^ oldWeightSaveFileName;
		DNNStates OldState;
	};
}
