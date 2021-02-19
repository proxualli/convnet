#include "dnncore.h"

using namespace std;
using namespace System::Windows::Media;
using namespace msclr::interop;

using namespace dnncore;

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
	SGDMomentum = 7,
	RAdam = 8
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

enum class Activations
{
	Abs = 0,
	BoundedRelu = 1,
	Clip = 2,
	Elu = 3,
	Exp = 4,
	FTS = 5,
	Gelu = 6,
	GeluErf = 7,
	HardLogistic = 8,
	HardSwish = 9,
	Linear = 10,
	Log = 11,
	Logistic = 12,
	LogLogistic = 13,
	LogSoftmax = 14,
	Mish = 15,
	Pow = 16,
	PRelu = 17,
	Relu = 18,
	Round = 19,
	Softmax = 20,
	SoftRelu = 21,
	Sqrt = 22,
	Square = 23,
	Swish = 24,
	Tanh = 25
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

enum class Position
{
	TopLeft = 0,
	TopRight = 1,
	BottomLeft = 2,
	BottomRight = 3,
	Center = 4
};

enum class Interpolation
{
	Cubic = 0,
	Linear = 1,
	Nearest = 2
};

struct TrainingRate
{
	size_t BatchSize;
	size_t Cycles;
	size_t Epochs;
	size_t EpochMultiplier;
	size_t DecayAfterEpochs;
	size_t Interpolation;
	size_t ColorAngle;
	Float ColorCast;
	Float Distortion;
	Float Dropout;
	Float Cutout;
	Float AutoAugment;
	Float MaximumRate;
	Float MinimumRate;
	Float L2Penalty;
	Float Momentum;
	Float DecayFactor;
	Float Scaling;
	Float Rotation;
	bool HorizontalFlip;
	bool VerticalFlip;

	TrainingRate::TrainingRate() :
		BatchSize(1),
		Cycles(1),
		Epochs(200),
		EpochMultiplier(1),
		DecayAfterEpochs(1),
		Interpolation(size_t(Interpolation::Cubic)),
		ColorAngle(0),
		ColorCast(Float(0)),
		Distortion(Float(0)),
		Dropout(Float(0)),
		Cutout(Float(0)),
		AutoAugment(Float(0)),
		MaximumRate(Float(0.05)),
		MinimumRate(Float(0.0001)),
		L2Penalty(Float(0.0005)),
		Momentum(Float(0.9)),
		DecayFactor(Float(1)),
		Scaling(Float(10.0)),
		Rotation(Float(10.0)),
		HorizontalFlip(false),
		VerticalFlip(false)
	{
	}

	TrainingRate::TrainingRate(const Float maximumRate, const size_t batchSize, const size_t cycles, const size_t epochs, const size_t epochMultiplier, const Float minimumRate, const Float L2penalty, const Float momentum, const Float decayFactor, const size_t decayAfterEpochs, const bool horizontalFlip, const bool verticalFlip, const Float dropout, const Float cutout, const Float autoAugment, const Float colorCast, const size_t colorAngle, const Float distortion, const size_t interpolation, const Float scaling, const Float rotation) :
		MaximumRate(maximumRate),
		BatchSize(batchSize),
		Cycles(cycles),
		Epochs(epochs),
		EpochMultiplier(epochMultiplier),
		MinimumRate(minimumRate),
		L2Penalty(L2penalty),
		Momentum(momentum),
		DecayFactor(decayFactor),
		DecayAfterEpochs(decayAfterEpochs),
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
	size_t Row;
	size_t Column;
	bool Error;
	std::string Message;

	CheckMsg(const size_t row = 0, const size_t column = 0, const std::string& message = "", const bool error = true) :
		Row(row),
		Column(column),
		Message(message),
		Error(error)
	{
	}
};

#define DNN_API extern "C" __declspec(dllimport)

DNN_API bool DNNStochasticEnabled();
DNN_API void DNNSetLocked(const bool locked);
DNN_API bool DNNSetLayerLocked(const size_t layerIndex, const bool locked);
DNN_API void DNNPersistOptimizer(const bool persist);
DNN_API void DNNDisableLocking(const bool disable);
DNN_API void DNNGetConfusionMatrix(const size_t costLayerIndex, std::vector<std::vector<size_t>>* confusionMatrix);
DNN_API void DNNGetLayerInputs(const size_t layerIndex, std::vector<size_t>* inputs);
DNN_API void DNNGetLayerInfo(const size_t layerIndex, size_t* inputsCount, LayerTypes* layerType, Activations* activationFunction, Costs* cost, std::string* name, std::string* description, size_t* neuronCount, size_t* weightCount, size_t* biasesCount, size_t* multiplier, size_t* groups, size_t* group, size_t* localSize, size_t* c, size_t* d, size_t* h, size_t* w, size_t* kernelH, size_t* kernelW, size_t* strideH, size_t* strideW, size_t* dilationH, size_t* dilationW, size_t* padD, size_t* padH, size_t* padW, Float* dropout, Float* labelTrue, Float* labelFalse, Float* weight, size_t* groupIndex, size_t* labelIndex, size_t* inputC, Float* alpha, Float* beta, Float* k, Algorithms* algorithm, Float* fH, Float* fW, bool* hasBias, bool* scaling, bool* acrossChannels, bool* locked, bool* lockable);
DNN_API void DNNSetNewEpochDelegate(void(*newEpoch)(size_t, size_t, size_t, bool, bool, Float, Float, Float, Float, size_t, Float, size_t, Float, Float, Float, size_t, Float, Float, Float, Float, Float, size_t, Float, Float, Float, size_t));
DNN_API void DNNModelDispose();
DNN_API bool DNNBatchNormalizationUsed();
DNN_API void DNNSetOptimizersHyperParameters(const Float adaDeltaEps, const Float adaGradEps, const Float adamEps, const Float adamBeta2, const Float adamaxEps, const Float adamaxBeta2, const Float rmsPropEps, const Float radamEps, const Float radamBeta1, const Float radamBeta2);
DNN_API void DNNResetWeights();
DNN_API void DNNResetLayerWeights(const size_t layerIndex);
DNN_API void DNNAddLearningRate(const bool clear, const size_t gotoEpoch, const Float maximumRate, const size_t bachSize, const size_t cycles, const size_t epochs, const size_t epochMultiplier, const Float minimumRate, const Float L2penalty, const Float momentum, const Float decayFactor, const size_t decayAfterEpochs, const bool horizontalFlip, const bool verticalFlip, const Float dropout, const Float cutout, const Float autoAugment, const Float colorCast, const size_t colorAngle, const Float distortion, const size_t interpolation, const Float scaling, const Float rotation);
DNN_API void DNNAddLearningRateSGDR(const bool clear, const size_t gotoEpoch, const Float maximumRate, const size_t bachSize, const size_t cycles, const size_t epochs, const size_t epochMultiplier, const Float minimumRate, const Float L2penalty, const Float momentum, const Float decayFactor, const size_t decayAfterEpochs, const bool horizontalFlip, const bool verticalFlip, const Float dropout, const Float cutout, const Float autoAugment, const Float colorCast, const size_t colorAngle, const Float distortion, const size_t interpolation, const Float scaling, const Float rotation);
DNN_API bool DNNLoadDataset();
DNN_API void DNNTraining();
DNN_API void DNNStop();
DNN_API void DNNPause();
DNN_API void DNNResume();
DNN_API void DNNTesting();
DNN_API void DNNGetTrainingInfo(size_t* currentCycle, size_t* totalCycles, size_t* currentEpoch, size_t* totalEpochs, bool* verticalMirror, bool* horizontalMirror, Float* dropout, Float* cutout, Float* autoAugment, Float* colorCast, size_t* colorAngle, Float* distortion, size_t* interpolation, Float* scaling, Float* rotation, size_t* sampleIndex, size_t* batchSize, Float* rate, Float* momentum, Float* l2Penalty, Float* avgTrainLoss, Float* trainErrorPercentage, size_t* trainErrors, Float* avgTestLoss, Float* testErrorPercentage, size_t* testErrors, Float* sampleSpeed, States* networkState, TaskStates* taskState);
DNN_API void DNNGetTestingInfo(size_t* batchSize, size_t* sampleIndex, Float* avgTestLoss, Float* testErrorPercentage, size_t* testErrors, Float* sampleSpeed, States* networkState, TaskStates* taskState);
DNN_API void DNNGetNetworkInfo(std::string* name, size_t* costIndex, size_t* costLayerCount, size_t* groupIndex, size_t* labelindex, size_t* hierarchies, bool* meanStdNormalization, Costs* lossFunction, Datasets* dataset, size_t* layerCount, size_t* trainingSamples, size_t* testingSamples, std::vector<Float>* meanTrainSet, std::vector<Float>* stdTrainSet);
DNN_API void DNNSetOptimizer(const Optimizers strategy);
DNN_API void DNNResetOptimizer();
DNN_API void DNNRefreshStatistics(const size_t layerIndex, std::string* description, Stats* neuronsStats, Stats* weightsStats, Stats* biasesStats, Float* fpropLayerTime, Float* bpropLayerTime, Float* updateLayerTime, Float* fpropTime, Float* bpropTime, Float* updateTime, bool* locked);
DNN_API bool DNNGetInputSnapShot(std::vector<Float>* snapshot, std::vector<size_t>* label);
DNN_API bool DNNCheckDefinition(std::string& definition, CheckMsg& checkMsg);
DNN_API int DNNLoadDefinition(const std::string& fileName, const Optimizers optimizer, CheckMsg& checkMsg);
DNN_API int DNNReadDefinition(const std::string& definition, const Optimizers optimizer, CheckMsg& checkMsg);
DNN_API void DNNDataprovider(const std::string& directory);
DNN_API int DNNLoadNetworkWeights(const std::string& fileName, const bool persistOptimizer);
DNN_API int DNNSaveNetworkWeights(const std::string& fileName, const bool persistOptimizer);
DNN_API int DNNLoadLayerWeights(const std::string& fileName, const size_t layerIndex, const bool persistOptimizer);
DNN_API int DNNSaveLayerWeights(const std::string& fileName, const size_t layerIndex, const bool persistOptimizer);
DNN_API void DNNGetLayerWeights(const size_t layerIndex, std::vector<Float>* weights, std::vector<Float>* biases);
DNN_API void DNNSetCostIndex(const size_t index);
DNN_API void DNNGetCostInfo(const size_t costIndex, size_t* trainErrors, Float* trainLoss, Float* avgTrainLoss, Float* trainErrorPercentage, size_t* testErrors, Float* testLoss, Float* avgTestLoss, Float* testErrorPercentage);
DNN_API void DNNGetImage(const size_t layer, const unsigned char fillColor, unsigned char* image);
DNN_API bool DNNSetFormat(const bool plain);

namespace dnncore
{
	Model::Model(String^ name, String^ fileName, DNNOptimizers optimizer)
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

		AdaGradEps = Float(1e-08);
		AdamEps = Float(1e-08);
		AdamBeta2 = Float(0.999);
		AdamaxEps = Float(1e-08);
		AdamaxBeta2 = Float(0.999);
		RAdamEps = Float(1e-08);
		RAdamBeta1 = Float(0.9);
		RAdamBeta2 = Float(0.999);
		RMSPropEps = Float(1e-08);

		GroupIndex = 0;
		LabelIndex = 0;
		CostIndex = 0;
		Multiplier = 1;

		DNNDataprovider(ToUnmanagedString(StorageDirectory));

		CheckMsg checkMsg;
		if (DNNLoadDefinition(ToUnmanagedString(fileName), (Optimizers)optimizer, checkMsg))
		{
			Optimizer = optimizer;

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

		auto confusionMatrix = std::vector<std::vector<size_t>>(classCount);
		for (size_t i = 0; i < classCount; i++)
			confusionMatrix[i] = std::vector<size_t>(classCount);

		DNNGetConfusionMatrix(CostIndex, &confusionMatrix);

		ConfusionMatrix = gcnew cli::array<cli::array<size_t>^>(int(classCount));
		for (int i = 0; i < classCount; i++)
		{
			ConfusionMatrix[i] = gcnew cli::array<size_t>(int(classCount));
			for (int j = 0; j < classCount; j++)
				ConfusionMatrix[i][j] = confusionMatrix[i][j];
		}
	}

	void Model::SetOptimizersHyperParameters(Float adaDeltaEps, Float adaGradEps, Float adamEps, Float adamBeta2, Float adamaxEps, Float adamaxBeta2, Float rmsPropEps, Float radamEps, Float radamBeta1, Float radamBeta2)
	{
		AdaDeltaEps = adaDeltaEps;
		AdaGradEps = adaGradEps;
		AdamEps = adamEps;
		AdamBeta2 = adamBeta2;
		AdamaxEps = adamaxEps;
		AdamaxBeta2 = adamaxBeta2;
		RMSPropEps = rmsPropEps;
		RAdamEps = radamEps;
		RAdamBeta1 = radamBeta1;
		RAdamBeta2 = radamBeta2;

		DNNSetOptimizersHyperParameters(adaDeltaEps, adaGradEps, adamEps, adamBeta2, adamaxEps, adamaxBeta2, rmsPropEps, radamEps, radamBeta1, radamBeta2);
	}

	bool Model::LoadDataset()
	{
		return DNNLoadDataset();
	}

	bool Model::BatchNormalizationUsed()
	{
		return DNNBatchNormalizationUsed();
	}

	void Model::UpdateLayerStatistics(LayerInformation^ info, size_t layerIndex, bool updateUI)
	{
		auto description = new std::string();
		auto neuronsStats = new Stats();
		auto weightsStats = new Stats();
		auto biasesStats = new Stats();
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

				if (totalSize <= INT_MAX)
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
			case DNNLayerTypes::Dense:
			{
				const auto width = info->BiasCount;
				const auto height = (info->WeightCount / info->BiasCount) + 3;
				const auto totalSize = width * height;
				auto pixelFormat = PixelFormats::Gray8;

				if (totalSize <= INT_MAX)
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

				if (totalSize <= INT_MAX)
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

	void Model::UpdateInputSnapshot(size_t C, size_t H, size_t W)
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
				for (size_t channel = 0; channel < C; channel++)
					for (size_t hw = 0; hw < HW; hw++)
						img[int((hw * C) + channel)] = pictureLoaded ? FloatSaturate((snapshot[hw + channel * HW] * StdTrainSet[channel]) + MeanTrainSet[channel]) : FloatSaturate(MeanTrainSet[channel]);
			else
				for (size_t channel = 0; channel < C; channel++)
					for (size_t hw = 0; hw < HW; hw++)
						img[int((hw * C) + channel)] = pictureLoaded ? FloatSaturate((snapshot[hw + channel * HW] + Float(2)) * 64) : FloatSaturate(128);

			auto outputImage = System::Windows::Media::Imaging::BitmapSource::Create(int(W), int(H), 96.0, 96.0, pixelFormat, nullptr, img, int(W) * ((pixelFormat.BitsPerPixel + 7) / 8));
			if (outputImage->CanFreeze)
				outputImage->Freeze();

			InputSnapshot = outputImage;
			Label = pictureLoaded ? LabelsCollection[int(LabelIndex)][int(labelVector[LabelIndex])] : System::String::Empty;
		}
	}

	LayerInformation^ Model::GetLayerInfo(size_t layerIndex, bool updateUI)
	{
		LayerInformation^ info = gcnew LayerInformation();

		auto layerType = new LayerTypes();
		auto activationFunction = new Activations();
		auto costFunction = new Costs();
		auto aName = new std::string();
		auto aDescription = new std::string();
		auto inputsCount = new size_t();
		auto neuronCount = new size_t();
		auto weightCount = new size_t();
		auto biasCount = new size_t();
		auto multiplier = new size_t();
		auto groups = new size_t();
		auto group = new size_t();
		auto localSize = new size_t();
		auto c = new size_t();
		auto d = new size_t();
		auto h = new size_t();
		auto w = new size_t();
		auto kernelH = new size_t();
		auto kernelW = new size_t();
		auto strideH = new size_t();
		auto strideW = new size_t();
		auto dilationH = new size_t();
		auto dilationW = new size_t();
		auto padD = new size_t();
		auto padH = new size_t();
		auto padW = new size_t();
		auto algorithm = new Algorithms();
		auto factorH = new Float();
		auto factorW = new Float();
		auto dropout = new Float();
		auto labelTrue = new Float();
		auto labelFalse = new Float();
		auto weight = new Float();
		auto labelIndex = new size_t();
		auto groupIndex = new size_t();
		auto inputC = new size_t();
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
		info->IsNormalizationLayer = info->LayerType == DNNLayerTypes::BatchNorm || info->LayerType == DNNLayerTypes::BatchNormHardLogistic || info->LayerType == DNNLayerTypes::BatchNormHardSwish || info->LayerType == DNNLayerTypes::BatchNormHardSwishDropout || info->LayerType == DNNLayerTypes::BatchNormRelu || info->LayerType == DNNLayerTypes::BatchNormReluDropout || info->LayerType == DNNLayerTypes::BatchNormSwish || info->LayerType == DNNLayerTypes::BatchNormMish || info->LayerType == DNNLayerTypes::BatchNormMishDropout;
		info->ActivationFunctionEnum = static_cast<DNNActivations>(*activationFunction);
		info->CostFunction = static_cast<DNNCosts>(*costFunction);
		info->InputCount = *inputsCount;
		std::vector<size_t>* inputs = new std::vector<size_t>();
		DNNGetLayerInputs(layerIndex, inputs);
		info->Inputs = gcnew System::Collections::Generic::List<size_t>();
		for each (size_t inputLayerIndex in *inputs)
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

	void Model::UpdateLayerInfo(size_t layerIndex, bool updateUI)
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
			auto cycle = new size_t();
			auto totalCycles = new size_t();
			auto epoch = new size_t();
			auto totalEpochs = new size_t();
			auto horizontalFlip = new bool();
			auto verticalFlip = new bool();
			auto dropout = new Float();
			auto cutout = new Float();
			auto autoAugment = new Float();
			auto colorCast = new Float();
			auto colorAngle = new size_t();
			auto distortion = new Float();
			auto interpolation = new size_t();
			auto scaling = new Float();
			auto rotation = new Float;
			auto sampleIndex = new size_t();
			auto rate = new Float();
			auto momentum = new Float();
			auto l2Penalty = new Float();
			auto batchSize = new size_t();
			auto avgTrainLoss = new Float();
			auto trainErrorPercentage = new Float();
			auto trainErrors = new size_t();
			auto avgTestLoss = new Float();
			auto testErrorPercentage = new Float();
			auto testErrors = new size_t();
			auto sampleSpeed = new Float();
			auto aState = new States();
			auto aTaskState = new TaskStates();

			DNNGetTrainingInfo(cycle, totalCycles, epoch, totalEpochs, horizontalFlip, verticalFlip, dropout, cutout, autoAugment, colorCast, colorAngle, distortion, interpolation, scaling, rotation, sampleIndex, batchSize, rate, momentum, l2Penalty, avgTrainLoss, trainErrorPercentage, trainErrors, avgTestLoss, testErrorPercentage, testErrors, sampleSpeed, aState, aTaskState);

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
			Interpolation = *interpolation;
			Scaling = *scaling;
			Rotation = *rotation;
			SampleIndex = *sampleIndex;
			Rate = *rate;
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

			TrainProgress(BatchSize, Cycle, TotalCycles, Epoch, TotalEpochs, HorizontalFlip, VerticalFlip, Dropout, Cutout, AutoAugment, ColorCast, ColorAngle, Distortion, Interpolation, Scaling, Rotation, SampleIndex, Rate, Momentum, L2Penalty, AvgTrainLoss, TrainErrorPercentage, Float(100) - TrainErrorPercentage, TrainErrors, AvgTestLoss, TestErrorPercentage, Float(100) - TestErrorPercentage, TestErrors, State, TaskState);

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
			auto batchSize = new size_t();
			auto sampleIndex = new size_t();
			auto avgTestLoss = new Float();
			auto testErrorPercentage = new Float();
			auto testErrors = new size_t();
			auto sampleSpeed = new Float();
			auto aState = new States();
			auto aTaskState = new TaskStates();

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

	void Model::ResetLayerWeights(size_t layerIndex)
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
		DNNSetOptimizer(static_cast<Optimizers>(strategy));
		Optimizer = strategy;
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

	void Model::SetCostIndex(size_t index)
	{
		DNNSetCostIndex(index);
		CostIndex = index;
		GroupIndex = CostLayers[CostIndex]->GroupIndex;
		LabelIndex = CostLayers[CostIndex]->LabelIndex;
		ClassCount = CostLayers[CostIndex]->ClassCount;
	}

	void Model::UpdateCostInfo(size_t index)
	{
		auto trainErrors = new size_t();
		auto trainLoss = new Float();
		auto avgTrainLoss = new Float();
		auto trainErrorPercentage = new Float();

		auto testErrors = new size_t();
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
		auto costFunction = new Costs();
		auto dataset = new Datasets();
		auto aCostIndex = new size_t();
		auto costLayersCount = new size_t();
		auto hierarchies = new size_t();
		auto groupIndex = new size_t();
		auto labelIndex = new size_t();
		auto layerCount = new size_t();
		auto trainingSamples = new size_t();
		auto testingSamples = new size_t();
		auto meanTrainSet = vector<Float>();
		auto stdTrainSet = vector<Float>();

		DNNGetNetworkInfo(aName, aCostIndex, costLayersCount, groupIndex, labelIndex, hierarchies, meanStdNormalization, costFunction, dataset, layerCount, trainingSamples, testingSamples, &meanTrainSet, &stdTrainSet);
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
		size_t costLayersCounter = 0;

		Layers = gcnew System::Collections::ObjectModel::ObservableCollection<LayerInformation^>();

		for (size_t layer = 0; layer < LayerCount; layer++)
		{
			Layers->Add(GetLayerInfo(layer, false));

			if (Layers[layer]->LayerType == DNNLayerTypes::Cost)
				CostLayers[costLayersCounter++] = gcnew DNNCostLayer(Layers[layer]->CostFunction, Layers[layer]->LayerIndex, Layers[layer]->GroupIndex, Layers[layer]->LabelIndex, Layers[layer]->NeuronCount, Layers[layer]->Name, Layers[layer]->Weight);
		}

		GroupIndex = CostLayers[CostIndex]->GroupIndex;
		LabelIndex = CostLayers[CostIndex]->LabelIndex;
		ClassCount = CostLayers[CostIndex]->ClassCount;
	}

	void Model::AddLearningRate(bool clear, size_t gotoEpoch, DNNTrainingRate^ rate)
	{
		DNNAddLearningRate(clear, gotoEpoch, rate->MaximumRate, rate->BatchSize, rate->Cycles, rate->Epochs, rate->EpochMultiplier, rate->MinimumRate, rate->L2Penalty, rate->Momentum, rate->DecayFactor, rate->DecayAfterEpochs, rate->HorizontalFlip, rate->VerticalFlip, rate->Dropout, rate->Cutout, rate->AutoAugment, rate->ColorCast, rate->ColorAngle, rate->Distortion, rate->Interpolation, rate->Scaling, rate->Rotation);
	}

	void Model::AddLearningRateSGDR(bool clear, size_t gotoEpoch, DNNTrainingRate^ rate)
	{
		DNNAddLearningRateSGDR(clear, gotoEpoch, rate->MaximumRate, rate->BatchSize, rate->Cycles, rate->Epochs, rate->EpochMultiplier, rate->MinimumRate, rate->L2Penalty, rate->Momentum, rate->DecayFactor, rate->DecayAfterEpochs, rate->HorizontalFlip, rate->VerticalFlip, rate->Dropout, rate->Cutout, rate->AutoAugment, rate->ColorCast, rate->ColorAngle, rate->Distortion, rate->Interpolation, rate->Scaling, rate->Rotation);
	}

	void Model::Start(bool training)
	{
		IsTraining = training;
		if (NewEpoch != nullptr)
			DNNSetNewEpochDelegate((void(*)(size_t, size_t, size_t, bool, bool, Float, Float, Float, Float, size_t, Float, size_t, Float, Float, Float, size_t, Float, Float, Float, Float, Float, size_t, Float, Float, Float, size_t))(Marshal::GetFunctionPointerForDelegate(NewEpoch).ToPointer()));
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
		CheckMsg checkMsg;
		
		std::string def = ToUnmanagedString(definition);

		DNNCheckDefinition(def, checkMsg);
				
		definition = ToManagedString(def);

		return gcnew DNNCheckMsg(checkMsg.Row, checkMsg.Column, ToManagedString(checkMsg.Message), checkMsg.Error, definition);
	}

	int Model::LoadDefinition(String^ fileName)
	{
		CheckMsg checkMsg;

		DNNModelDispose();
		DNNDataprovider(ToUnmanagedString(StorageDirectory));

		GC::Collect(GC::MaxGeneration, GCCollectionMode::Forced, true, true);

		if (DNNLoadDefinition(ToUnmanagedString(fileName), (Optimizers)Optimizer, checkMsg))
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

	void Model::SetLayerLocked(size_t layerIndex, bool locked)
	{
		DNNSetLayerLocked(layerIndex, locked);
	}

	int Model::LoadWeights(String^ fileName, bool persist)
	{
		int ret = DNNLoadNetworkWeights(ToUnmanagedString(fileName), persist);

		for (size_t layerIndex = 0; layerIndex < LayerCount; layerIndex++)
			UpdateLayerStatistics(Layers[layerIndex], layerIndex, layerIndex == SelectedIndex);

		return ret;
	}

	int Model::SaveWeights(String^ fileName, bool persist)
	{
		return DNNSaveNetworkWeights(ToUnmanagedString(fileName), persist);
	}

	int Model::LoadLayerWeights(String^ fileName, size_t layerIndex)
	{
		int ret = DNNLoadLayerWeights(ToUnmanagedString(fileName).c_str(), layerIndex, false);

		if (ret == 0)
			UpdateLayerStatistics(Layers[layerIndex], layerIndex, layerIndex == SelectedIndex);

		return ret;
	}

	int Model::SaveLayerWeights(String^ fileName, size_t layerIndex)
	{
		return DNNSaveLayerWeights(ToUnmanagedString(fileName), layerIndex, false);
	}

	bool Model::StochasticEnabled()
	{
		return DNNStochasticEnabled();
	}
}
