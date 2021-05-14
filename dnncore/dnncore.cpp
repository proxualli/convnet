#include "dnncore.h"


using namespace std;
using namespace System::Windows::Media;
using namespace msclr::interop;

using namespace dnncore;

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
		AdaDelta = 0,
		AdaGrad = 1,
		Adam = 2,
		Adamax = 3,
		NAG = 4,
		RMSProp = 5,
		SGD = 6,
		SGDMomentum = 7
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
		LeCunNormal = 3,
		LeCunUniform = 4,
		Normal = 5,
		TruncatedNormal = 6,
		Uniform = 7,
		XavierNormal = 8,
		XavierUniform = 9
	};

	enum class LayerTypes
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
		LayerNorm = 31,
		LayerNormRelu = 32,
		LocalResponseNorm = 33,
		Max = 34,
		MaxPooling = 35,
		Min = 36,
		Multiply = 37,
		PartialDepthwiseConvolution = 38,
		Resampling = 39,
		Substract = 40
	};

	enum class Activations
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
		mobilenetv3 = 1,
		resnet = 2,
		shufflenetv2 = 3
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
		Float Eps;
		UInt BatchSize;
		UInt Cycles;
		UInt Epochs;
		UInt EpochMultiplier;
		Float MaximumRate;
		Float MinimumRate;
		UInt DecayAfterEpochs;
		Float DecayFactor;
		bool HorizontalFlip;
		bool VerticalFlip;
		Float Dropout;
		Float Cutout;
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
			Eps(Float(1E-08)),
			BatchSize(1),
			Cycles(1),
			Epochs(200),
			EpochMultiplier(1),
			MaximumRate(Float(0.05)),
			MinimumRate(Float(0.0001)),
			DecayAfterEpochs(1),
			DecayFactor(Float(1)),
			HorizontalFlip(false),
			VerticalFlip(false),
			Dropout(Float(0)),
			Cutout(Float(0)),
			AutoAugment(Float(0)),
			ColorCast(Float(0)),
			ColorAngle(0),
			Distortion(Float(0)),
			Interpolation(Interpolations::Cubic),
			Scaling(Float(10.0)),
			Rotation(Float(10.0))
		{
		}

		TrainingRate::TrainingRate(const Optimizers optimizer, const Float momentum, const Float beta2, const Float l2penalty, const Float eps, const UInt batchSize, const UInt cycles, const UInt epochs, const UInt epochMultiplier, const Float maximumRate, const Float minimumRate, const UInt decayAfterEpochs, const Float decayFactor, const bool horizontalFlip, const bool verticalFlip, const Float dropout, const Float cutout, const Float autoAugment, const Float colorCast, const UInt colorAngle, const Float distortion, const dnn::Interpolations interpolation, const Float scaling, const Float rotation) :
			Optimizer(optimizer),
			Momentum(momentum),
			Beta2(beta2),
			L2Penalty(l2penalty),
			Eps(eps),
			BatchSize(batchSize),
			Cycles(cycles),
			Epochs(epochs),
			EpochMultiplier(epochMultiplier),
			MaximumRate(maximumRate),
			MinimumRate(minimumRate),
			DecayAfterEpochs(decayAfterEpochs),
			DecayFactor(decayFactor),
			HorizontalFlip(horizontalFlip),
			VerticalFlip(verticalFlip),
			Dropout(dropout),
			Cutout(cutout),
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

		Stats() :
			Mean(0),
			StdDev(0),
			Min(0),
			Max(0)
		{
		}

		Stats(const Float mean, const Float stddev, const Float min, const Float  max) :
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
}

#define DNN_API extern "C" __declspec(dllimport)

DNN_API bool DNNStochasticEnabled();
DNN_API void DNNSetLocked(const bool locked);
DNN_API bool DNNSetLayerLocked(const UInt layerIndex, const bool locked);
DNN_API void DNNPersistOptimizer(const bool persist);
DNN_API void DNNDisableLocking(const bool disable);
DNN_API void DNNGetConfusionMatrix(const UInt costLayerIndex, std::vector<std::vector<UInt>>* confusionMatrix);
DNN_API void DNNGetLayerInputs(const UInt layerIndex, std::vector<UInt>* inputs);
DNN_API void DNNGetLayerInfo(const UInt layerIndex, UInt* inputsCount, dnn::LayerTypes* layerType, dnn::Activations* activationFunction, dnn::Costs* cost, std::string* name, std::string* description, UInt* neuronCount, UInt* weightCount, UInt* biasesCount, UInt* multiplier, UInt* groups, UInt* group, UInt* localSize, UInt* c, UInt* d, UInt* h, UInt* w, UInt* kernelH, UInt* kernelW, UInt* strideH, UInt* strideW, UInt* dilationH, UInt* dilationW, UInt* padD, UInt* padH, UInt* padW, Float* dropout, Float* labelTrue, Float* labelFalse, Float* weight, UInt* groupIndex, UInt* labelIndex, UInt* inputC, Float* alpha, Float* beta, Float* k, dnn::Algorithms* algorithm, Float* fH, Float* fW, bool* hasBias, bool* scaling, bool* acrossChannels, bool* locked, bool* lockable);
DNN_API void DNNSetNewEpochDelegate(void(*newEpoch)(UInt, UInt, UInt, UInt, Float, Float, bool, bool, Float, Float, Float, Float, UInt, Float, UInt, Float, Float, Float, UInt, Float, Float, Float, Float, Float, UInt, Float, Float, Float, UInt));
DNN_API void DNNModelDispose();
DNN_API bool DNNBatchNormalizationUsed();
DNN_API void DNNResetWeights();
DNN_API void DNNResetLayerWeights(const UInt layerIndex);
DNN_API void DNNAddLearningRate(const bool clear, const UInt gotoEpoch, const dnn::Optimizers optimizer, const Float momentum, const Float beta2, const Float L2penalty, const Float eps, const UInt batchSize, const UInt cycles, const UInt epochs, const UInt epochMultiplier, const Float maximumRate, const Float minimumRate, const Float decayFactor, const UInt decayAfterEpochs, const bool horizontalFlip, const bool verticalFlip, const Float dropout, const Float cutout, const Float autoAugment, const Float colorCast, const UInt colorAngle, const Float distortion, const dnn::Interpolations interpolation, const Float scaling, const Float rotation);
DNN_API void DNNAddLearningRateSGDR(const bool clear, const UInt gotoEpoch, const dnn::Optimizers optimizer, const Float momentum, const Float beta2, const Float L2penalty, const Float eps, const UInt batchSize, const UInt cycles, const UInt epochs, const UInt epochMultiplier, const Float maximumRate, const Float minimumRate, const Float decayFactor, const UInt decayAfterEpochs, const bool horizontalFlip, const bool verticalFlip, const Float dropout, const Float cutout, const Float autoAugment, const Float colorCast, const UInt colorAngle, const Float distortion, const dnn::Interpolations interpolation, const Float saling, const Float rotation);
DNN_API bool DNNLoadDataset();
DNN_API void DNNTraining();
DNN_API void DNNStop();
DNN_API void DNNPause();
DNN_API void DNNResume();
DNN_API void DNNTesting();
DNN_API void DNNGetTrainingInfo(UInt* currentCycle, UInt* totalCycles, UInt* currentEpoch, UInt* totalEpochs, bool* verticalMirror, bool* horizontalMirror, Float* dropout, Float* cutout, Float* autoAugment, Float* colorCast, UInt* colorAngle, Float* distortion, dnn::Interpolations* interpolation, Float* scaling, Float* rotation, UInt* sampleIndex, UInt* batchSize, Float* rate, dnn::Optimizers* optimizer, Float* momentum, Float* l2Penalty, Float* avgTrainLoss, Float* trainErrorPercentage, UInt* trainErrors, Float* avgTestLoss, Float* testErrorPercentage, UInt* testErrors, Float* sampleSpeed, dnn::States* networkState, dnn::TaskStates* taskState);
DNN_API void DNNGetTestingInfo(UInt* batchSize, UInt* sampleIndex, Float* avgTestLoss, Float* testErrorPercentage, UInt* testErrors, Float* sampleSpeed, dnn::States* networkState, dnn::TaskStates* taskState);
DNN_API void DNNGetModelInfo(std::string* name, UInt* costIndex, UInt* costLayerCount, UInt* groupIndex, UInt* labelindex, UInt* hierarchies, bool* meanStdNormalization, dnn::Costs* lossFunction, dnn::Datasets* dataset, UInt* layerCount, UInt* trainingSamples, UInt* testingSamples, std::vector<Float>* meanTrainSet, std::vector<Float>* stdTrainSet);
DNN_API void DNNSetOptimizer(const dnn::Optimizers strategy);
DNN_API void DNNResetOptimizer();
DNN_API void DNNRefreshStatistics(const UInt layerIndex, std::string* description, dnn::Stats* neuronsStats, dnn::Stats* weightsStats, dnn::Stats* biasesStats, Float* fpropLayerTime, Float* bpropLayerTime, Float* updateLayerTime, Float* fpropTime, Float* bpropTime, Float* updateTime, bool* locked);
DNN_API bool DNNGetInputSnapShot(std::vector<Float>* snapshot, std::vector<UInt>* label);
DNN_API bool DNNCheckDefinition(std::string& definition, dnn::CheckMsg& checkMsg);
DNN_API int DNNLoadDefinition(const std::string& fileName,dnn::CheckMsg& checkMsg);
DNN_API int DNNReadDefinition(const std::string& definition, dnn::CheckMsg& checkMsg);
DNN_API void DNNDataprovider(const std::string& directory);
DNN_API int DNNLoadWeights(const std::string& fileName, const bool persistOptimizer);
DNN_API int DNNSaveWeights(const std::string& fileName, const bool persistOptimizer);
DNN_API int DNNLoadLayerWeights(const std::string& fileName, const UInt layerIndex, const bool persistOptimizer);
DNN_API int DNNSaveLayerWeights(const std::string& fileName, const UInt layerIndex, const bool persistOptimizer);
DNN_API void DNNGetLayerWeights(const UInt layerIndex, std::vector<Float>* weights, std::vector<Float>* biases);
DNN_API void DNNSetCostIndex(const UInt index);
DNN_API void DNNGetCostInfo(const UInt costIndex, UInt* trainErrors, Float* trainLoss, Float* avgTrainLoss, Float* trainErrorPercentage, UInt* testErrors, Float* testLoss, Float* avgTestLoss, Float* testErrorPercentage);
DNN_API void DNNGetImage(const UInt layer, const dnn::Byte fillColor, dnn::Byte* image);
DNN_API bool DNNSetFormat(const bool plain);
DNN_API dnn::Optimizers GetOptimizer();

namespace dnncore
{
	Model::Model(String^ name, String^ fileName)
	{
		Name = name;
		Duration = gcnew System::Diagnostics::Stopwatch();
		
		sb = gcnew System::Text::StringBuilder();
		State = DNNStates::Idle;
		OldState = DNNStates::Idle;
		TaskState = dnncore::DNNTaskStates::Stopped;
		MeanTrainSet = gcnew cli::array<Float>(3);
		StdTrainSet = gcnew cli::array<Float>(3);
		StorageDirectory = String::Concat(Environment::GetFolderPath(Environment::SpecialFolder::MyDocuments), "\\convnet\\");
		DatasetsDirectory = String::Concat(StorageDirectory, "datasets\\");
		DefinitionsDirectory = String::Concat(StorageDirectory, "definitions\\");

	    GroupIndex = 0;
		LabelIndex = 0;
		CostIndex = 0;
		Multiplier = 1;

		DNNDataprovider(ToUnmanagedString(StorageDirectory));

		dnn::CheckMsg checkMsg;
		if (DNNLoadDefinition(ToUnmanagedString(fileName), checkMsg))
		{
			DNNLoadDataset();

			System::IO::StreamReader^ reader = gcnew System::IO::StreamReader(fileName, true);
			DefinitionDocument = reader->ReadToEnd();
			reader->Close();

			ApplyParameters();

			WorkerTimer = gcnew System::Timers::Timer(1000.0);
			WorkerTimer->Elapsed += gcnew System::Timers::ElapsedEventHandler(this, &dnncore::Model::OnElapsed);
		}
		else
		{
			throw std::exception(checkMsg.Message.c_str());
		}
	}

	Model::~Model()
	{
		DNNModelDispose();
		WorkerTimer->Close();
	}

	void Model::SetPersistOptimizer(bool persist)
	{
		DNNPersistOptimizer(persist);
		PersistOptimizer = persist;
	}

	void Model::SetDisableLocking(bool disable)
	{
		DNNDisableLocking(disable);
		DisableLocking = disable;
	}

	void Model::GetConfusionMatrix()
	{
		const auto classCount = CostLayers[CostIndex]->ClassCount;

		auto confusionMatrix = std::vector<std::vector<UInt>>(classCount);
		for (UInt i = 0; i < classCount; i++)
			confusionMatrix[i] = std::vector<UInt>(classCount);

		DNNGetConfusionMatrix(CostIndex, &confusionMatrix);

		ConfusionMatrix = gcnew cli::array<cli::array<UInt>^>(int(classCount));
		for (int i = 0; i < classCount; i++)
		{
			ConfusionMatrix[i] = gcnew cli::array<UInt>(int(classCount));
			for (int j = 0; j < classCount; j++)
				ConfusionMatrix[i][j] = confusionMatrix[i][j];
		}
	}

	bool Model::LoadDataset()
	{
		return DNNLoadDataset();
	}

	bool Model::BatchNormalizationUsed()
	{
		return DNNBatchNormalizationUsed();
	}

	void Model::UpdateLayerStatistics(LayerInformation^ info, UInt layerIndex, bool updateUI)
	{
		auto description = new std::string();
		auto neuronsStats = new dnn::Stats();
		auto weightsStats = new dnn::Stats();
		auto biasesStats = new dnn::Stats();
		auto fpropLayerTime = new Float();
		auto bpropLayerTime = new Float();
		auto updateLayerTime = new Float();
		auto fpropTiming = new Float();
		auto bpropTiming = new Float();
		auto updateTiming = new Float();
		auto islocked = new bool();

		DNNRefreshStatistics(layerIndex, description, neuronsStats, weightsStats, biasesStats, fpropLayerTime, bpropLayerTime, updateLayerTime, fpropTiming, bpropTiming, updateTiming, islocked);

		info->Description = ToManagedString(*description);

		info->NeuronsStdDev = neuronsStats->StdDev;
		info->NeuronsMean = neuronsStats->Mean;
		info->NeuronsMin = neuronsStats->Min;
		info->NeuronsMax = neuronsStats->Max;
		info->WeightsStdDev = weightsStats->StdDev;
		info->WeightsMean = weightsStats->Mean;
		info->WeightsMin = weightsStats->Min;
		info->WeightsMax = weightsStats->Max;
		info->BiasesStdDev = biasesStats->StdDev;
		info->BiasesMean = biasesStats->Mean;
		info->BiasesMin = biasesStats->Min;
		info->BiasesMax = biasesStats->Max;

		info->FPropLayerTime = *fpropLayerTime;
		info->BPropLayerTime = *bpropLayerTime;
		info->UpdateLayerTime = *updateLayerTime;
		fpropTime = *fpropTiming;
		bpropTime = *bpropTiming;
		updateTime = *updateTiming;

		info->LockUpdate = info->Lockable ? Nullable<bool>(*islocked) : Nullable<bool>(false);

		delete description;
		delete neuronsStats;
		delete weightsStats;
		delete biasesStats;
		delete fpropLayerTime;
		delete bpropLayerTime;
		delete updateLayerTime;
		delete fpropTiming;
		delete bpropTiming;
		delete updateTiming;
		delete islocked;

		if (updateUI)
		{
			switch (info->LayerType)
			{
			case DNNLayerTypes::Input:
				UpdateInputSnapshot(info->C, info->H, info->W);
				break;

			case DNNLayerTypes::Convolution:
			case DNNLayerTypes::ConvolutionTranspose:
			case DNNLayerTypes::DepthwiseConvolution:
			case DNNLayerTypes::PartialDepthwiseConvolution:
			{
				const auto depthwise = info->LayerType == DNNLayerTypes::DepthwiseConvolution || info->LayerType == DNNLayerTypes::PartialDepthwiseConvolution;
				const auto width = (info->C * info->KernelH) + info->C + 1;
				const auto height = depthwise ? 2 + info->KernelW + 1 : (info->InputC == 3 ? info->KernelW + 4 : 2 + (info->InputC / info->Groups) * (info->KernelW + 1));
				const auto totalSize = (!depthwise && info->InputC == 3) ? 3 * width * height : width * height;
				auto pixelFormat = (!depthwise && info->InputC == 3) ? PixelFormats::Rgb24 : PixelFormats::Gray8;

				if (totalSize > 0 && totalSize <= INT_MAX)
				{
					auto img = gcnew cli::array<Byte>(int(totalSize));
					pin_ptr<Byte> p = &img[0];
					Byte* np = p;

					DNNGetImage(info->LayerIndex, 100, np);

					auto outputImage = System::Windows::Media::Imaging::BitmapSource::Create(int(width), int(height), 96.0, 96.0, pixelFormat, nullptr, img, int(width) * ((pixelFormat.BitsPerPixel + 7) / 8));
					if (outputImage->CanFreeze)
						outputImage->Freeze();

					info->WeightsSnapshotX = int(width * BlockSize);
					info->WeightsSnapshotY = int(height * BlockSize);
					info->WeightsSnapshot = outputImage;

					GC::Collect(GC::MaxGeneration, GCCollectionMode::Forced, true, true);
				}
			}
			break;


			case DNNLayerTypes::BatchNorm:
			case DNNLayerTypes::BatchNormHardLogistic:
			case DNNLayerTypes::BatchNormHardSwish:
			case DNNLayerTypes::BatchNormHardSwishDropout:
			case DNNLayerTypes::BatchNormRelu:
			case DNNLayerTypes::BatchNormReluDropout:
			case DNNLayerTypes::BatchNormSwish:
			case DNNLayerTypes::BatchNormSwishDropout:
			case DNNLayerTypes::BatchNormTanhExp:
			case DNNLayerTypes::BatchNormTanhExpDropout:
			case DNNLayerTypes::Dense:
			case DNNLayerTypes::LayerNorm:
			{
				const auto width = info->BiasCount;
				const auto height = (info->WeightCount / info->BiasCount) + 3;
				const auto totalSize = width * height;
				auto pixelFormat = PixelFormats::Gray8;

				if (totalSize > 0 && totalSize <= INT_MAX)
				{
					auto img = gcnew cli::array<Byte>(int(totalSize));
					pin_ptr<Byte> p = &img[0];
					Byte* np = p;

					DNNGetImage(info->LayerIndex, 100, np);

					auto outputImage = System::Windows::Media::Imaging::BitmapSource::Create(int(width), int(height), 96.0, 96.0, pixelFormat, nullptr, img, int(width) * ((pixelFormat.BitsPerPixel + 7) / 8));
					if (outputImage->CanFreeze)
						outputImage->Freeze();

					info->WeightsSnapshotX = int(width * BlockSize);
					info->WeightsSnapshotY = int(height * BlockSize);
					info->WeightsSnapshot = outputImage;

					GC::Collect(GC::MaxGeneration, GCCollectionMode::Forced, true, true);
				}
			}
			break;

			case DNNLayerTypes::Activation:
			{
				const auto width = info->WeightCount;
				const auto height = 4;
				const auto totalSize = width * height;
				auto pixelFormat = PixelFormats::Gray8;

				if (totalSize > 0 && totalSize <= INT_MAX)
				{
					auto img = gcnew cli::array<Byte>(int(totalSize));
					pin_ptr<Byte> p = &img[0];
					Byte* np = p;

					DNNGetImage(info->LayerIndex, 100, np);

					auto outputImage = System::Windows::Media::Imaging::BitmapSource::Create(int(width), int(height), 96.0, 96.0, pixelFormat, nullptr, img, int(width) * ((pixelFormat.BitsPerPixel + 7) / 8));
					if (outputImage->CanFreeze)
						outputImage->Freeze();

					info->WeightsSnapshotX = int(width * BlockSize);
					info->WeightsSnapshotY = int(height * BlockSize);
					info->WeightsSnapshot = outputImage;

					GC::Collect(GC::MaxGeneration, GCCollectionMode::Forced, true, true);
				}
			}
			break;
			}
		}
	}

	void Model::UpdateInputSnapshot(UInt C, UInt H, UInt W)
	{
		const auto totalSize = C * H * W;
		auto snapshot = std::vector<Float>(totalSize);
		auto labelVector = std::vector<UInt64>(Hierarchies);

		const auto pictureLoaded = DNNGetInputSnapShot(&snapshot, &labelVector);

		if (totalSize > 0)
		{
			auto img = gcnew cli::array<Byte>(int(totalSize));
			auto pixelFormat = C == 3 ? PixelFormats::Rgb24 : PixelFormats::Gray8;
			const auto HW = H * W;

			if (MeanStdNormalization)
				for (UInt channel = 0; channel < C; channel++)
					for (UInt hw = 0; hw < HW; hw++)
						img[int((hw * C) + channel)] = pictureLoaded ? FloatSaturate((snapshot[hw + channel * HW] * StdTrainSet[channel]) + MeanTrainSet[channel]) : FloatSaturate(MeanTrainSet[channel]);
			else
				for (UInt channel = 0; channel < C; channel++)
					for (UInt hw = 0; hw < HW; hw++)
						img[int((hw * C) + channel)] = pictureLoaded ? FloatSaturate((snapshot[hw + channel * HW] + Float(2)) * 64) : FloatSaturate(128);

			auto outputImage = System::Windows::Media::Imaging::BitmapSource::Create(int(W), int(H), 96.0, 96.0, pixelFormat, nullptr, img, int(W) * ((pixelFormat.BitsPerPixel + 7) / 8));
			if (outputImage->CanFreeze)
				outputImage->Freeze();

			InputSnapshot = outputImage;
			Label = pictureLoaded ? LabelsCollection[int(LabelIndex)][int(labelVector[LabelIndex])] : System::String::Empty;
		}
	}

	LayerInformation^ Model::GetLayerInfo(UInt layerIndex, bool updateUI)
	{
		LayerInformation^ info = gcnew LayerInformation();

		auto layerType = new dnn::LayerTypes();
		auto activationFunction = new dnn::Activations();
		auto costFunction = new dnn::Costs();
		auto aName = new std::string();
		auto aDescription = new std::string();
		auto inputsCount = new UInt();
		auto neuronCount = new UInt();
		auto weightCount = new UInt();
		auto biasCount = new UInt();
		auto multiplier = new UInt();
		auto groups = new UInt();
		auto group = new UInt();
		auto localSize = new UInt();
		auto c = new UInt();
		auto d = new UInt();
		auto h = new UInt();
		auto w = new UInt();
		auto kernelH = new UInt();
		auto kernelW = new UInt();
		auto strideH = new UInt();
		auto strideW = new UInt();
		auto dilationH = new UInt();
		auto dilationW = new UInt();
		auto padD = new UInt();
		auto padH = new UInt();
		auto padW = new UInt();
		auto algorithm = new dnn::Algorithms();
		auto factorH = new Float();
		auto factorW = new Float();
		auto dropout = new Float();
		auto labelTrue = new Float();
		auto labelFalse = new Float();
		auto weight = new Float();
		auto labelIndex = new UInt();
		auto groupIndex = new UInt();
		auto inputC = new UInt();
		auto alpha = new Float();
		auto beta = new Float();
		auto k = new Float;
		auto hasBias = new bool();
		auto scaling = new bool();
		auto acrossChannels = new bool();
		auto lockable = new bool();
		auto locked = new bool();

		DNNGetLayerInfo(layerIndex, inputsCount, layerType, activationFunction, costFunction, aName, aDescription, neuronCount, weightCount, biasCount, multiplier, groups, group, localSize, c, d, h, w, kernelH, kernelW, strideH, strideW, dilationH, dilationW, padD, padH, padW, dropout, labelTrue, labelFalse, weight, groupIndex, labelIndex, inputC, alpha, beta, k, algorithm, factorH, factorW, hasBias, scaling, acrossChannels, locked, lockable);

		info->LayerIndex = layerIndex;
		info->LayerType = static_cast<DNNLayerTypes>(*layerType);
		info->IsNormalizationLayer = info->LayerType == DNNLayerTypes::BatchNorm || info->LayerType == DNNLayerTypes::BatchNormHardLogistic || info->LayerType == DNNLayerTypes::BatchNormHardSwish || info->LayerType == DNNLayerTypes::BatchNormHardSwishDropout || info->LayerType == DNNLayerTypes::BatchNormMish || info->LayerType == DNNLayerTypes::BatchNormMishDropout || info->LayerType == DNNLayerTypes::BatchNormRelu || info->LayerType == DNNLayerTypes::BatchNormReluDropout || info->LayerType == DNNLayerTypes::BatchNormSwish || info->LayerType == DNNLayerTypes::BatchNormSwishDropout || info->LayerType == DNNLayerTypes::BatchNormTanhExp || info->LayerType == DNNLayerTypes::BatchNormTanhExpDropout || info->LayerType == DNNLayerTypes::LayerNorm;
		info->ActivationFunctionEnum = static_cast<DNNActivations>(*activationFunction);
		info->CostFunction = static_cast<DNNCosts>(*costFunction);
		info->InputCount = *inputsCount;
		std::vector<UInt>* inputs = new std::vector<UInt>();
		DNNGetLayerInputs(layerIndex, inputs);
		info->Inputs = gcnew System::Collections::Generic::List<UInt>();
		for each (UInt inputLayerIndex in *inputs)
			info->Inputs->Add(inputLayerIndex);
		info->Name = ToManagedString(*aName);
		info->Description = ToManagedString(*aDescription);
		info->NeuronCount = *neuronCount;
		info->WeightCount = *weightCount;
		info->HasWeights = info->WeightCount > 0;
		info->HasBias = *hasBias;
		info->BiasCount = *biasCount;
		info->Multiplier = *multiplier;
		info->Groups = *groups;
		info->Group = *group;
		info->LocalSize = *localSize;
		info->C = *c;
		info->D = *d;
		info->H = *h;
		info->W = *w;
		info->HW = (*h) * (*w);
		info->DilationH = *dilationH;
		info->DilationW = *dilationW;
		info->KernelH = *kernelH;
		info->KernelW = *kernelW;
		info->KernelHW = info->KernelH * info->KernelW;
		info->StrideH = *strideH;
		info->StrideW = *strideW;
		info->PadD = *padD;
		info->PadH = *padH;
		info->PadW = *padW;
		info->Dropout = *dropout;
		info->HasDropout = *dropout > 0;
		info->InputC = *inputC;
		info->Weight = *weight;
		info->GroupIndex = *groupIndex;
		info->LabelIndex = *labelIndex;
		info->Alpha = *alpha;
		info->Beta = *beta;
		info->K = *k;
		info->Algorithm = static_cast<DNNAlgorithms>(*algorithm);
		info->FactorH = *factorH;
		info->FactorW = *factorW;
		info->Scaling = info->IsNormalizationLayer ? *scaling : false;
		info->AcrossChannels = *acrossChannels;
		info->Lockable = *lockable;
		info->LockUpdate = *lockable ? Nullable<bool>(*locked) : Nullable<bool>(false);

		delete aName;
		delete aDescription;
		delete inputs;
		delete layerType;
		delete activationFunction;
		delete costFunction;
		delete neuronCount;
		delete weightCount;
		delete biasCount;
		delete multiplier;
		delete groups;
		delete group;
		delete localSize;
		delete c;
		delete h;
		delete w;
		delete kernelH;
		delete kernelW;
		delete strideH;
		delete strideW;
		delete dilationH;
		delete dilationW;
		delete padH;
		delete padW;
		delete dropout;
		delete labelTrue;
		delete labelFalse;
		delete weight;
		delete groupIndex;
		delete labelIndex;
		delete inputC;
		delete alpha;
		delete beta;
		delete k;
		delete algorithm;
		delete factorH;
		delete factorW;
		delete hasBias;
		delete scaling;
		delete acrossChannels;
		delete locked;
		delete lockable;

		if (updateUI)
			UpdateLayerStatistics(info, layerIndex, updateUI);

		return info;
	}

	void Model::UpdateLayerInfo(UInt layerIndex, bool updateUI)
	{
		UpdateLayerStatistics(Layers[layerIndex], layerIndex, updateUI);
	}

	void Model::OnElapsed(Object^, System::Timers::ElapsedEventArgs^)
	{
		sb->Length = 0;
		if (Duration->Elapsed.Days > 0)
		{
			if (Duration->Elapsed.Days == 1)
				sb->AppendFormat("{0:D} day {1:D2}:{2:D2}:{3:D2}", Duration->Elapsed.Days, Duration->Elapsed.Hours, Duration->Elapsed.Minutes, Duration->Elapsed.Seconds);
			else
				sb->AppendFormat("{0:D} days {1:D2}:{2:D2}:{3:D2}", Duration->Elapsed.Days, Duration->Elapsed.Hours, Duration->Elapsed.Minutes, Duration->Elapsed.Seconds);
		}
		else
			sb->AppendFormat("{0:D2}:{1:D2}:{2:D2}", Duration->Elapsed.Hours, Duration->Elapsed.Minutes, Duration->Elapsed.Seconds);
		DurationString = sb->ToString();

		if (IsTraining)
		{
			auto cycle = new UInt();
			auto totalCycles = new UInt();
			auto epoch = new UInt();
			auto totalEpochs = new UInt();
			auto horizontalFlip = new bool();
			auto verticalFlip = new bool();
			auto dropout = new Float();
			auto cutout = new Float();
			auto autoAugment = new Float();
			auto colorCast = new Float();
			auto colorAngle = new UInt();
			auto distortion = new Float();
			auto interpolation = new dnn::Interpolations();
			auto scaling = new Float();
			auto rotation = new Float;
			auto sampleIndex = new UInt();
			auto rate = new Float();
			auto momentum = new Float();
			auto optimizer = new dnn::Optimizers();
			auto l2Penalty = new Float();
			auto batchSize = new UInt();
			auto avgTrainLoss = new Float();
			auto trainErrorPercentage = new Float();
			auto trainErrors = new UInt();
			auto avgTestLoss = new Float();
			auto testErrorPercentage = new Float();
			auto testErrors = new UInt();
			auto sampleSpeed = new Float();
			auto aState = new dnn::States();
			auto aTaskState = new dnn::TaskStates();

			DNNGetTrainingInfo(cycle, totalCycles, epoch, totalEpochs, horizontalFlip, verticalFlip, dropout, cutout, autoAugment, colorCast, colorAngle, distortion, interpolation, scaling, rotation, sampleIndex, batchSize, rate, optimizer, momentum, l2Penalty, avgTrainLoss, trainErrorPercentage, trainErrors, avgTestLoss, testErrorPercentage, testErrors, sampleSpeed, aState, aTaskState);

			Cycle = *cycle;
			TotalCycles = *totalCycles;
			Epoch = *epoch;
			TotalEpochs = *totalEpochs;
			HorizontalFlip = *horizontalFlip;
			VerticalFlip = *verticalFlip;
			Dropout = *dropout;
			Cutout = *cutout;
			AutoAugment = *autoAugment;
			ColorCast = *colorCast;
			ColorAngle = *colorAngle;
			Distortion = *distortion;
			Interpolation = static_cast<dnncore::DNNInterpolations>(*interpolation);
			Scaling = *scaling;
			Rotation = *rotation;
			SampleIndex = *sampleIndex;
			Rate = *rate;
			if (Optimizer != static_cast<dnncore::DNNOptimizers>(*optimizer))
				Optimizer = static_cast<dnncore::DNNOptimizers>(*optimizer);
			Momentum = *momentum;
			L2Penalty = *l2Penalty;
			BatchSize = *batchSize;
			AvgTrainLoss = *avgTrainLoss;
			TrainErrorPercentage = *trainErrorPercentage;
			TrainErrors = *trainErrors;
			AvgTestLoss = *avgTestLoss;
			TestErrorPercentage = *testErrorPercentage;
			TestErrors = *testErrors;
			SampleRate = *sampleSpeed;;
			State = static_cast<dnncore::DNNStates>(*aState);
			TaskState = static_cast<dnncore::DNNTaskStates>(*aTaskState);


			AdjustedTrainingSamplesCount = TrainingSamples % BatchSize == 0 ? TrainingSamples : ((TrainingSamples / BatchSize) + 1) * BatchSize;
			AdjustedTestingSamplesCount = TestingSamples % BatchSize == 0 ? TestingSamples : ((TestingSamples / BatchSize) + 1) * BatchSize;

			TrainProgress(Optimizer, BatchSize, Cycle, TotalCycles, Epoch, TotalEpochs, HorizontalFlip, VerticalFlip, Dropout, Cutout, AutoAugment, ColorCast, ColorAngle, Distortion, Interpolation, Scaling, Rotation, SampleIndex, Rate, Momentum, L2Penalty, AvgTrainLoss, TrainErrorPercentage, Float(100) - TrainErrorPercentage, TrainErrors, AvgTestLoss, TestErrorPercentage, Float(100) - TestErrorPercentage, TestErrors, State, TaskState);

			if (State != OldState)
			{
				OldState = State;
				SampleRate = Float(0);
			}

			delete cycle;
			delete totalCycles;
			delete epoch;
			delete totalEpochs;
			delete horizontalFlip;
			delete verticalFlip;
			delete dropout;
			delete cutout;
			delete autoAugment;
			delete colorCast;
			delete colorAngle;
			delete distortion;
			delete interpolation;
			delete scaling;
			delete rotation;
			delete sampleIndex;
			delete rate;
			delete optimizer;
			delete momentum;
			delete l2Penalty;
			delete batchSize;
			delete avgTrainLoss;
			delete trainErrorPercentage;
			delete trainErrors;
			delete avgTestLoss;
			delete testErrorPercentage;
			delete testErrors;
			delete sampleSpeed;
			delete aState;
			delete aTaskState;
		}
		else
		{
			auto batchSize = new UInt();
			auto sampleIndex = new UInt();
			auto avgTestLoss = new Float();
			auto testErrorPercentage = new Float();
			auto testErrors = new UInt();
			auto sampleSpeed = new Float();
			auto aState = new dnn::States();
			auto aTaskState = new dnn::TaskStates();

			DNNGetTestingInfo(batchSize, sampleIndex, avgTestLoss, testErrorPercentage, testErrors, sampleSpeed, aState, aTaskState);

			BatchSize = *batchSize;
			SampleIndex = *sampleIndex;
			AvgTestLoss = *avgTestLoss;
			TestErrorPercentage = *testErrorPercentage;
			TestErrors = *testErrors;
			State = static_cast<dnncore::DNNStates>(*aState);
			TaskState = static_cast<dnncore::DNNTaskStates>(*aTaskState);
			SampleRate = *sampleSpeed;

			AdjustedTestingSamplesCount = TestingSamples % BatchSize == 0 ? TestingSamples : ((TestingSamples / BatchSize) + 1) * BatchSize;

			TestProgress(BatchSize, SampleIndex, AvgTestLoss, TestErrorPercentage, Float(100) - TestErrorPercentage, TestErrors, State, TaskState);

			if (State != OldState)
			{
				OldState = State;
				SampleRate = Float(0);
			}

			delete batchSize;
			delete sampleIndex;
			delete avgTestLoss;
			delete testErrorPercentage;
			delete testErrors;
			delete sampleSpeed;
			delete aState;
			delete aTaskState;
		}
	}

	void Model::ResetWeights()
	{
		DNNResetWeights();
	}

	void Model::ResetLayerWeights(UInt layerIndex)
	{
		DNNResetLayerWeights(layerIndex);
	}

	bool Model::SetFormat(bool plain)
	{
		bool ret = DNNSetFormat(plain);

		if (ret)
			PlainFormat = plain;

		return ret;
	}

	void Model::SetOptimizer(DNNOptimizers strategy)
	{
		if (strategy != Optimizer)
		{
			DNNSetOptimizer(static_cast<dnn::Optimizers>(strategy));
			Optimizer = strategy;
		}
	}

	void Model::ResetOptimizer()
	{
		DNNResetOptimizer();
	}

	cli::array<String^>^ Model::GetTextLabels(String^ fileName)
	{
		int lines = 0;
		cli::array<String^>^ list;
		try
		{
			StreamReader^ streamReader = File::OpenText(fileName);

			try
			{
				String^ str;
				while ((str = streamReader->ReadLine()) != nullptr)
					lines++;
			}
			finally
			{
				delete streamReader;
			}
			list = gcnew cli::array<String^>(lines);

			lines = 0;
			streamReader = File::OpenText(fileName);
			try
			{
				String^ str;
				while ((str = streamReader->ReadLine()) != nullptr)
				{
					list[lines] = gcnew String(str);
					lines++;
				}
			}
			finally
			{
				delete streamReader;
			}
		}
		catch (Exception^)
		{
			/*if (dynamic_cast<FileNotFoundException^>(e))
				Console::WriteLine("file '{0}' not found", fileName);
			else
				Console::WriteLine("problem reading file '{0}'", fileName);*/
		}

		return list;

	}

	void Model::SetCostIndex(UInt index)
	{
		DNNSetCostIndex(index);
		CostIndex = index;
		GroupIndex = CostLayers[CostIndex]->GroupIndex;
		LabelIndex = CostLayers[CostIndex]->LabelIndex;
		ClassCount = CostLayers[CostIndex]->ClassCount;
	}

	void Model::UpdateCostInfo(UInt index)
	{
		auto trainErrors = new UInt();
		auto trainLoss = new Float();
		auto avgTrainLoss = new Float();
		auto trainErrorPercentage = new Float();

		auto testErrors = new UInt();
		auto testLoss = new Float();
		auto avgTestLoss = new Float();
		auto testErrorPercentage = new Float();

		DNNGetCostInfo(index, trainErrors, trainLoss, avgTrainLoss, trainErrorPercentage, testErrors, testLoss, avgTestLoss, testErrorPercentage);

		CostLayers[index]->TrainErrors = *trainErrors;
		CostLayers[index]->TrainLoss = *trainLoss;
		CostLayers[index]->AvgTrainLoss = *avgTrainLoss;
		CostLayers[index]->TrainErrorPercentage = *trainErrorPercentage;
		CostLayers[index]->TrainAccuracy = Float(100) - *trainErrorPercentage;

		CostLayers[index]->TestErrors = *testErrors;
		CostLayers[index]->TestLoss = *testLoss;
		CostLayers[index]->AvgTestLoss = *avgTestLoss;
		CostLayers[index]->TestErrorPercentage = *testErrorPercentage;
		CostLayers[index]->TestAccuracy = Float(100) - *testErrorPercentage;

		delete trainErrors;
		delete trainLoss;
		delete avgTrainLoss;
		delete trainErrorPercentage;

		delete testErrors;
		delete testLoss;
		delete avgTestLoss;
		delete testErrorPercentage;
	}

	void Model::ApplyParameters()
	{
		auto aName = new std::string();
		auto meanStdNormalization = new bool();
		auto costFunction = new dnn::Costs();
		auto dataset = new dnn::Datasets();
		auto aCostIndex = new UInt();
		auto costLayersCount = new UInt();
		auto hierarchies = new UInt();
		auto groupIndex = new UInt();
		auto labelIndex = new UInt();
		auto layerCount = new UInt();
		auto trainingSamples = new UInt();
		auto testingSamples = new UInt();
		auto meanTrainSet = vector<Float>();
		auto stdTrainSet = vector<Float>();

		DNNGetModelInfo(aName, aCostIndex, costLayersCount, groupIndex, labelIndex, hierarchies, meanStdNormalization, costFunction, dataset, layerCount, trainingSamples, testingSamples, &meanTrainSet, &stdTrainSet);
		Name = ToManagedString(*aName);
		CostIndex = *aCostIndex;
		CostLayersCount = *costLayersCount;
		GroupIndex = *groupIndex;
		LabelIndex = *labelIndex;
		Hierarchies = *hierarchies;
		MeanStdNormalization = *meanStdNormalization;
		CostFunction = safe_cast<DNNCosts>(*costFunction);
		Dataset = safe_cast<DNNDatasets>(*dataset);
		LayerCount = *layerCount;
		TrainingSamples = *trainingSamples;
		TestingSamples = *testingSamples;

		delete aName;
		delete meanStdNormalization;
		delete costFunction;
		delete dataset;
		delete aCostIndex;
		delete costLayersCount;
		delete hierarchies;
		delete groupIndex;
		delete labelIndex;
		delete layerCount;
		delete trainingSamples;
		delete testingSamples;

		LabelsCollection = gcnew cli::array<cli::array<String^>^>(int(Hierarchies));

		switch (Dataset)
		{
		case DNNDatasets::tinyimagenet:
			LabelsCollection[0] = GetTextLabels(String::Concat(DatasetsDirectory, Dataset.ToString() + "\\classnames.txt"));
			/*LabelsCollection[0] = gcnew cli::array<String^>(200);
			for (int i = 0; i < 200; i++)
				LabelsCollection[0][i] = i.ToString();*/
			if (meanTrainSet.size() >= 3)
			{
				for (int i = 0; i < 3; i++)
				{
					MeanTrainSet[i] = meanTrainSet[i];
					StdTrainSet[i] = stdTrainSet[i];
				}
			}
			break;

		case DNNDatasets::cifar10:
			LabelsCollection[0] = GetTextLabels(String::Concat(DatasetsDirectory, Dataset.ToString() + "\\batches.meta.txt"));
			if (meanTrainSet.size() >= 3)
			{
				for (int i = 0; i < 3; i++)
				{
					MeanTrainSet[i] = meanTrainSet[i];
					StdTrainSet[i] = stdTrainSet[i];
				}
			}
			break;

		case DNNDatasets::cifar100:
			LabelsCollection[0] = GetTextLabels(String::Concat(DatasetsDirectory, Dataset.ToString() + "\\coarse_label_names.txt"));
			LabelsCollection[1] = GetTextLabels(String::Concat(DatasetsDirectory, Dataset.ToString() + "\\fine_label_names.txt"));
			if (meanTrainSet.size() >= 3)
			{
				for (int i = 0; i < 3; i++)
				{
					MeanTrainSet[i] = meanTrainSet[i];
					StdTrainSet[i] = stdTrainSet[i];
				}
			}
			break;

		case DNNDatasets::fashionmnist:
			LabelsCollection[0] = GetTextLabels(String::Concat(DatasetsDirectory, Dataset.ToString() + "\\batches.meta.txt"));
			if (meanTrainSet.size() >= 1)
			{
				for (int i = 0; i < 1; i++)
				{
					MeanTrainSet[i] = meanTrainSet[i];
					StdTrainSet[i] = stdTrainSet[i];
				}
			}
			break;

		case DNNDatasets::mnist:
			LabelsCollection[0] = gcnew cli::array<String^>(10);
			for (int i = 0; i < 10; i++)
				LabelsCollection[0][i] = i.ToString();
			if (meanTrainSet.size() >= 1)
			{
				for (int i = 0; i < 1; i++)
				{
					MeanTrainSet[i] = meanTrainSet[i];
					StdTrainSet[i] = stdTrainSet[i];
				}
			}
			break;
		}

		CostLayers = gcnew cli::array<DNNCostLayer^>(int(CostLayersCount));
		UInt costLayersCounter = 0;

		Layers = gcnew System::Collections::ObjectModel::ObservableCollection<LayerInformation^>();

		for (UInt layer = 0; layer < LayerCount; layer++)
		{
			Layers->Add(GetLayerInfo(layer, false));

			if (Layers[layer]->LayerType == DNNLayerTypes::Cost)
				CostLayers[costLayersCounter++] = gcnew DNNCostLayer(Layers[layer]->CostFunction, Layers[layer]->LayerIndex, Layers[layer]->GroupIndex, Layers[layer]->LabelIndex, Layers[layer]->NeuronCount, Layers[layer]->Name, Layers[layer]->Weight);
		}

		GroupIndex = CostLayers[CostIndex]->GroupIndex;
		LabelIndex = CostLayers[CostIndex]->LabelIndex;
		ClassCount = CostLayers[CostIndex]->ClassCount;

		Optimizer = static_cast<DNNOptimizers>(GetOptimizer());
	}

	void Model::AddLearningRate(bool clear, UInt gotoEpoch, DNNTrainingRate^ rate)
	{
		DNNAddLearningRate(clear, gotoEpoch, static_cast<dnn::Optimizers>(rate->Optimizer), rate->Momentum, rate->Beta2, rate->L2Penalty, rate->Eps, rate->BatchSize, rate->Cycles, rate->Epochs, rate->EpochMultiplier, rate->MaximumRate, rate->MinimumRate, rate->DecayFactor, rate->DecayAfterEpochs, rate->HorizontalFlip, rate->VerticalFlip, rate->Dropout, rate->Cutout, rate->AutoAugment, rate->ColorCast, rate->ColorAngle, rate->Distortion, static_cast<dnn::Interpolations>(rate->Interpolation), rate->Scaling, rate->Rotation);
	}

	void Model::AddLearningRateSGDR(bool clear, UInt gotoEpoch, DNNTrainingRate^ rate)
	{
		DNNAddLearningRateSGDR(clear, gotoEpoch, static_cast<dnn::Optimizers>(rate->Optimizer), rate->Momentum, rate->Beta2, rate->L2Penalty, rate->Eps, rate->BatchSize, rate->Cycles, rate->Epochs, rate->EpochMultiplier, rate->MaximumRate, rate->MinimumRate, rate->DecayFactor, rate->DecayAfterEpochs, rate->HorizontalFlip, rate->VerticalFlip, rate->Dropout, rate->Cutout, rate->AutoAugment, rate->ColorCast, rate->ColorAngle, rate->Distortion, static_cast<dnn::Interpolations>(rate->Interpolation), rate->Scaling, rate->Rotation);
	}

	void Model::Start(bool training)
	{
		IsTraining = training;
		if (NewEpoch != nullptr)
			DNNSetNewEpochDelegate((void(*)(UInt, UInt, UInt, UInt, Float, Float, bool, bool, Float, Float, Float, Float, UInt, Float, UInt, Float, Float, Float, UInt, Float, Float, Float, Float, Float, UInt, Float, Float, Float, UInt))(Marshal::GetFunctionPointerForDelegate(NewEpoch).ToPointer()));
		SampleRate = Float(0);
		State = DNNStates::Idle;
		if (training)
			DNNTraining();
		else 
			DNNTesting();
		TaskState = DNNTaskStates::Running;
		WorkerTimer->Start();
		Duration->Start();
	}

	void Model::Stop()
	{
		SampleRate = Float(0);
		Duration->Reset();
		DNNStop();
		WorkerTimer->Stop();
		State = DNNStates::Completed;
		TaskState = DNNTaskStates::Stopped;
	}

	void Model::Pause()
	{
		WorkerTimer->Stop();
		Duration->Stop();
		DNNPause();
		TaskState = DNNTaskStates::Paused;
	}

	void Model::Resume()
	{
		DNNResume();
		Duration->Start();
		WorkerTimer->Start();
		TaskState = DNNTaskStates::Running;
	}

	DNNCheckMsg^ Model::CheckDefinition(String^ definition)
	{
		dnn::CheckMsg checkMsg;
		
		std::string def = ToUnmanagedString(definition);

		DNNCheckDefinition(def, checkMsg);
				
		definition = ToManagedString(def);

		return gcnew DNNCheckMsg(checkMsg.Row, checkMsg.Column, ToManagedString(checkMsg.Message), checkMsg.Error, definition);
	}

	int Model::LoadDefinition(String^ fileName)
	{
		dnn::CheckMsg checkMsg;

		DNNModelDispose();
		DNNDataprovider(ToUnmanagedString(StorageDirectory));

		GC::Collect(GC::MaxGeneration, GCCollectionMode::Forced, true, true);

		if (DNNLoadDefinition(ToUnmanagedString(fileName), checkMsg))
		{
			DNNLoadDataset();

			auto reader = gcnew System::IO::StreamReader(fileName, true);
			DefinitionDocument = reader->ReadToEnd();
			reader->Close();

			DNNResetWeights();
			ApplyParameters();
		}
		else
			throw std::exception(checkMsg.Message.c_str());

		GC::Collect(GC::MaxGeneration, GCCollectionMode::Forced, true, true);

		return 1;
	}

	void Model::SetLocked(bool locked)
	{
		DNNSetLocked(locked);
		for (int i = 0; i < LayerCount; i++)
			if (Layers[i]->Lockable)
				Layers[i]->LockUpdate = locked;
	}

	void Model::SetLayerLocked(UInt layerIndex, bool locked)
	{
		DNNSetLayerLocked(layerIndex, locked);
	}

	int Model::LoadWeights(String^ fileName, bool persist)
	{
		int ret = DNNLoadWeights(ToUnmanagedString(fileName), persist);

		Optimizer = static_cast<DNNOptimizers>(GetOptimizer());

		for (UInt layerIndex = 0; layerIndex < LayerCount; layerIndex++)
			UpdateLayerStatistics(Layers[layerIndex], layerIndex, layerIndex == SelectedIndex);

		return ret;
	}

	int Model::SaveWeights(String^ fileName, bool persist)
	{
		return DNNSaveWeights(ToUnmanagedString(fileName), persist);
	}

	int Model::LoadLayerWeights(String^ fileName, UInt layerIndex)
	{
		int ret = DNNLoadLayerWeights(ToUnmanagedString(fileName).c_str(), layerIndex, false);

		if (ret == 0)
			UpdateLayerStatistics(Layers[layerIndex], layerIndex, layerIndex == SelectedIndex);

		return ret;
	}

	int Model::SaveLayerWeights(String^ fileName, UInt layerIndex)
	{
		return DNNSaveLayerWeights(ToUnmanagedString(fileName), layerIndex, false);
	}

	bool Model::StochasticEnabled()
	{
		return DNNStochasticEnabled();
	}
}