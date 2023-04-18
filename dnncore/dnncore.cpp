#include "dnncore.h"


using namespace std;
using namespace System::Windows::Media;
using namespace msclr::interop;

using namespace dnncore;



#define DNN_API extern "C" __declspec(dllimport)

DNN_API bool DNNStochasticEnabled();
DNN_API void DNNSetLocked(const bool locked);
DNN_API bool DNNSetLayerLocked(const UInt layerIndex, const bool locked);
DNN_API void DNNPersistOptimizer(const bool persist);
DNN_API void DNNDisableLocking(const bool disable);
DNN_API void DNNGetConfusionMatrix(const UInt costLayerIndex, std::vector<std::vector<UInt>>* confusionMatrix);
DNN_API void DNNGetLayerInputs(const UInt layerIndex, std::vector<UInt>* inputs);
DNN_API void DNNGetLayerInfo(const UInt layerIndex, dnn::LayerInfo* info);
DNN_API void DNNSetNewEpochDelegate(void(*newEpoch)(UInt, UInt, UInt, UInt, Float, Float, Float, bool, bool, Float, Float, bool, Float, Float, UInt, Float, UInt, Float, Float, Float, UInt, UInt, UInt, Float, Float, Float, Float, Float, Float, UInt, Float, Float, Float, UInt));
DNN_API void DNNModelDispose();
DNN_API bool DNNBatchNormalizationUsed();
DNN_API void DNNResetWeights();
DNN_API void DNNResetLayerWeights(const UInt layerIndex);
DNN_API void DNNAddTrainingRate(const dnn::TrainingRate& rate, const bool clear, const UInt gotoEpoch, const UInt trainSamples);
DNN_API void DNNAddTrainingRateSGDR(const dnn::TrainingRate& rate, const bool clear, const UInt gotoEpoch, const UInt trainSamples);
DNN_API void DNNClearTrainingStrategies();
DNN_API void DNNSetUseTrainingStrategy(const bool enable);
DNN_API void DNNAddTrainingStrategy(const dnn::TrainingStrategy& strategy);
DNN_API bool DNNLoadDataset();
DNN_API void DNNTraining();
DNN_API void DNNStop();
DNN_API void DNNPause();
DNN_API void DNNResume();
DNN_API void DNNTesting();
DNN_API void DNNGetTrainingInfo(dnn::TrainingInfo* info);
DNN_API void DNNGetTestingInfo(dnn::TestingInfo* info);
DNN_API void DNNGetModelInfo(dnn::ModelInfo* info);
DNN_API void DNNSetOptimizer(const dnn::Optimizers strategy);
DNN_API void DNNResetOptimizer();
DNN_API void DNNRefreshStatistics(const UInt layerIndex, dnn::StatsInfo* info);
DNN_API bool DNNGetInputSnapShot(std::vector<Float>* snapshot, std::vector<UInt>* label);
DNN_API bool DNNCheck(std::string& definition, dnn::CheckMsg& checkMsg);
DNN_API int DNNLoad(const std::string& fileName,dnn::CheckMsg& checkMsg);
DNN_API int DNNRead(const std::string& definition, dnn::CheckMsg& checkMsg);
DNN_API void DNNDataprovider(const std::string& directory);
DNN_API bool DNNSetShuffleCount(const UInt count);
DNN_API void DNNDataproviderDispose();
DNN_API int DNNLoadWeights(const std::string& fileName, const bool persistOptimizer);
DNN_API int DNNSaveWeights(const std::string& fileName, const bool persistOptimizer);
DNN_API int DNNLoadLayerWeights(const std::string& fileName, const UInt layerIndex, const bool persistOptimizer);
DNN_API int DNNSaveLayerWeights(const std::string& fileName, const UInt layerIndex, const bool persistOptimizer);
DNN_API void DNNGetLayerWeights(const UInt layerIndex, std::vector<Float>* weights, std::vector<Float>* biases);
DNN_API void DNNSetCostIndex(const UInt index);
DNN_API void DNNGetCostInfo(const UInt costIndex, dnn::CostInfo* info);
DNN_API void DNNGetImage(const UInt layer, const dnn::Byte fillColor, dnn::Byte* image);
DNN_API bool DNNSetFormat(const bool plain);
DNN_API void DNNGetResolution(UInt* N, UInt* C, UInt* D, UInt* H, UInt* W);
DNN_API dnn::Optimizers GetOptimizer();

namespace dnncore
{
	DNNModel::DNNModel(String^ definition)
	{
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
		if (DNNRead(ToUnmanagedString(definition), checkMsg))
		{
			DNNLoadDataset();
			Definition = definition;
			ApplyParameters();

			WorkerTimer = gcnew System::Timers::Timer(1000.0);
			WorkerTimer->Elapsed += gcnew System::Timers::ElapsedEventHandler(this, &dnncore::DNNModel::OnElapsed);
		}
		else
		{
			throw std::exception(checkMsg.Message.c_str());
		}
	}

	DNNModel::~DNNModel()
	{
		DNNModelDispose();
		WorkerTimer->Close();
		DNNDataproviderDispose();
	}
	
	void DNNModel::SetPersistOptimizer(bool persist)
	{
		DNNPersistOptimizer(persist);
		PersistOptimizer = persist;
	}

	void DNNModel::SetDisableLocking(bool disable)
	{
		DNNDisableLocking(disable);
		DisableLocking = disable;
	}

	void DNNModel::GetConfusionMatrix()
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

	bool DNNModel::LoadDataset()
	{
		return DNNLoadDataset();
	}

	bool DNNModel::SetShuffleCount(UInt count)
	{
		return DNNSetShuffleCount(count);
	}

	bool DNNModel::BatchNormalizationUsed()
	{
		return DNNBatchNormalizationUsed();
	}

	DNNLayerInfo^ DNNModel::GetLayerInfo(DNNLayerInfo^ infoManaged, UInt layerIndex)
	{
		if (infoManaged == nullptr)
			infoManaged = gcnew DNNLayerInfo();

		auto infoNative = new dnn::LayerInfo();
		DNNGetLayerInfo(layerIndex, infoNative);

		infoManaged->Name = ToManagedString(infoNative->Name);
		infoManaged->Description = ToManagedString(infoNative->Description);

		const auto layerType = safe_cast<DNNLayerTypes>(infoNative->LayerType);
		infoManaged->LayerType = layerType;
		infoManaged->IsNormalizationLayer =
			layerType == DNNLayerTypes::BatchNorm ||
			layerType == DNNLayerTypes::BatchNormActivation ||
			layerType == DNNLayerTypes::BatchNormActivationDropout ||
			layerType == DNNLayerTypes::BatchNormRelu ||
			layerType == DNNLayerTypes::LayerNorm;

		infoManaged->Activation = safe_cast<DNNActivations>(infoNative->Activation);
		infoManaged->Algorithm = safe_cast<DNNAlgorithms>(infoNative->Algorithm);
		infoManaged->Cost = safe_cast<DNNCosts>(infoNative->Cost);
		infoManaged->NeuronCount = infoNative->NeuronCount;
		infoManaged->WeightCount = infoNative->WeightCount;
		infoManaged->BiasCount = infoNative->BiasesCount;
		infoManaged->LayerIndex = layerIndex; // infoNative->LayerIndex;

		infoManaged->InputCount = infoNative->InputsCount;
		std::vector<UInt>* inputs = new std::vector<UInt>();
		DNNGetLayerInputs(layerIndex, inputs);
		infoManaged->Inputs = gcnew System::Collections::Generic::List<UInt>();
		for each (UInt index in *inputs)
			infoManaged->Inputs->Add(index);

		infoManaged->C = infoNative->C;
		infoManaged->D = infoNative->D;
		infoManaged->H = infoNative->H;
		infoManaged->W = infoNative->W;
		infoManaged->PadD = infoNative->PadD;
		infoManaged->PadH = infoNative->PadH;
		infoManaged->PadW = infoNative->PadW;
		infoManaged->KernelH = infoNative->KernelH;
		infoManaged->KernelW = infoNative->KernelW;
		infoManaged->StrideH = infoNative->StrideH;
		infoManaged->StrideW = infoNative->StrideW;
		infoManaged->DilationH = infoNative->DilationH;
		infoManaged->DilationW = infoNative->DilationW;
		infoManaged->Multiplier = infoNative->Multiplier;
		infoManaged->Groups = infoNative->Groups;
		infoManaged->Group = infoNative->Group;
		infoManaged->LocalSize = infoNative->LocalSize;
		infoManaged->Dropout = infoNative->Dropout;
		infoManaged->Weight = infoNative->Weight;
		infoManaged->GroupIndex = infoNative->GroupIndex;
		infoManaged->LabelIndex = infoNative->LabelIndex;
		infoManaged->InputC = infoNative->InputC;
		infoManaged->Alpha = infoNative->Alpha;
		infoManaged->Beta = infoNative->Beta;
		infoManaged->K = infoNative->K;
		infoManaged->FactorH = infoNative->fH;
		infoManaged->FactorW = infoNative->fW;
		infoManaged->HasBias = infoNative->HasBias;
		infoManaged->Scaling = infoManaged->IsNormalizationLayer ? infoNative->Scaling : false;
		infoManaged->AcrossChannels = infoNative->AcrossChannels;
		infoManaged->LockUpdate = infoNative->Lockable ? Nullable<bool>(infoNative->Locked) : Nullable<bool>(false);
		infoManaged->Lockable = infoNative->Lockable;

		delete infoNative;

		return infoManaged;
	}

	void DNNModel::UpdateLayerStatistics(DNNLayerInfo^ info, UInt layerIndex, bool updateUI)
	{
		auto statsInfo = new dnn::StatsInfo();
		DNNRefreshStatistics(layerIndex, statsInfo);

		info->Description = ToManagedString(statsInfo->Description);
		info->NeuronsStats = gcnew DNNStats(statsInfo->NeuronsStats);
		info->WeightsStats = gcnew DNNStats(statsInfo->WeightsStats);
		info->BiasesStats = gcnew DNNStats(statsInfo->BiasesStats);
		info->FPropLayerTime = statsInfo->FPropLayerTime;
		info->BPropLayerTime = statsInfo->BPropLayerTime;
		info->UpdateLayerTime = statsInfo->UpdateLayerTime;
		fpropTime = statsInfo->FPropTime;
		bpropTime = statsInfo->BPropTime;
		updateTime = statsInfo->UpdateTime;
		info->LockUpdate = info->Lockable ? Nullable<bool>(statsInfo->Locked) : Nullable<bool>(false);

		delete statsInfo;

		if (updateUI)
		{
			switch (info->LayerType)
			{
			case DNNLayerTypes::Input:
			{
				const auto totalSize = info->C * info->H * info->W;
				auto snapshot = std::vector<Float>(totalSize);
				auto labelVector = std::vector<UInt64>(Hierarchies);

				const auto pictureLoaded = DNNGetInputSnapShot(&snapshot, &labelVector);

				if (totalSize > 0)
				{
					auto img = gcnew cli::array<Byte>(int(totalSize));
					auto pixelFormat = info->C == 3 ? PixelFormats::Rgb24 : PixelFormats::Gray8;
					const auto HW = info->H * info->W;

					if (MeanStdNormalization)
						for (UInt channel = 0; channel < info->C; channel++)
							for (UInt hw = 0; hw < HW; hw++)
								img[int((hw * info->C) + channel)] = pictureLoaded ? FloatSaturate((snapshot[hw + channel * HW] * StdTrainSet[channel]) + MeanTrainSet[channel]) : FloatSaturate(MeanTrainSet[channel]);
					else
						for (UInt channel = 0; channel < info->C; channel++)
							for (UInt hw = 0; hw < HW; hw++)
								img[int((hw * info->C) + channel)] = pictureLoaded ? FloatSaturate((snapshot[hw + channel * HW] + Float(2)) * 64) : FloatSaturate(128);

					auto outputImage = System::Windows::Media::Imaging::BitmapSource::Create(int(info->W), int(info->H), 96.0, 96.0, pixelFormat, nullptr, img, int(info->W) * ((pixelFormat.BitsPerPixel + 7) / 8));
					if (outputImage->CanFreeze)
						outputImage->Freeze();

					InputSnapshot = outputImage;
					Label = pictureLoaded ? LabelsCollection[int(LabelIndex)][int(labelVector[LabelIndex])] : System::String::Empty;
				}
			}
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

					DNNGetImage(info->LayerIndex, BackgroundColor, np);

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
			case DNNLayerTypes::BatchNormActivation:
			case DNNLayerTypes::BatchNormActivationDropout:
			case DNNLayerTypes::BatchNormRelu:
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

					DNNGetImage(info->LayerIndex, BackgroundColor, np);

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

			case DNNLayerTypes::PRelu:
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

					DNNGetImage(info->LayerIndex, BackgroundColor, np);

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

	void DNNModel::UpdateLayerInfo(UInt layerIndex, bool updateUI)
	{
		if (layerIndex == 0)
			GetLayerInfo(Layers[layerIndex], layerIndex);
		
		UpdateLayerStatistics(Layers[layerIndex], layerIndex, updateUI);
	}

	void DNNModel::OnElapsed(Object^, System::Timers::ElapsedEventArgs^)
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
			auto info = new dnn::TrainingInfo;
			DNNGetTrainingInfo(info);

			TotalCycles = info->TotalCycles;
			TotalEpochs = info->TotalEpochs;
			Cycle = info->Cycle;
			Epoch = info->Epoch;;
			SampleIndex = info->SampleIndex;
			
			Rate = info->Rate;
			if (Optimizer != safe_cast<dnncore::DNNOptimizers>(info->Optimizer))
				Optimizer = safe_cast<dnncore::DNNOptimizers>(info->Optimizer);

			Momentum = info->Momentum;
			Beta2 = info->Beta2;
			L2Penalty = info->L2Penalty;
			Gamma = info->Gamma;
			Dropout = info->Dropout;
			BatchSize = info->BatchSize;
			Height = info->Height;
			Width = info->Width;
			PadH = info->PadH;
			PadW = info->PadW;

			HorizontalFlip = info->HorizontalFlip;
			VerticalFlip = info->VerticalFlip;
			InputDropout = info->InputDropout;
			Cutout = info->Cutout;
			CutMix = info->CutMix;
			AutoAugment = info->AutoAugment;
			ColorCast = info->ColorCast;
			ColorAngle = info->ColorAngle;
			Distortion = info->Distortion;
			Interpolation = safe_cast<dnncore::DNNInterpolations>(info->Interpolation);
			Scaling = info->Scaling;
			Rotation = info->Rotation;
			
			AvgTrainLoss = info->AvgTrainLoss;
			TrainErrorPercentage = info->TrainErrorPercentage;
			TrainErrors = info->TrainErrors;
			AvgTestLoss = info->AvgTestLoss;
			TestErrorPercentage = info->TestErrorPercentage;
			TestErrors = info->TestErrors;
			
			SampleRate = info->SampleSpeed;

			State = safe_cast<dnncore::DNNStates>(info->State);
			TaskState = safe_cast<dnncore::DNNTaskStates>(info->TaskState);
			
			delete info;

			AdjustedTrainingSamplesCount = TrainingSamples % BatchSize == 0 ? TrainingSamples : ((TrainingSamples / BatchSize) + 1) * BatchSize;
			AdjustedTestingSamplesCount = TestingSamples % BatchSize == 0 ? TestingSamples : ((TestingSamples / BatchSize) + 1) * BatchSize;

			TrainProgress(Optimizer, BatchSize, Cycle, TotalCycles, Epoch, TotalEpochs, HorizontalFlip, VerticalFlip, InputDropout, Cutout, CutMix, AutoAugment, ColorCast, ColorAngle, Distortion, Interpolation, Scaling, Rotation, SampleIndex, Rate, Momentum, Beta2, Gamma, L2Penalty, Dropout, AvgTrainLoss, TrainErrorPercentage, Float(100) - TrainErrorPercentage, TrainErrors, AvgTestLoss, TestErrorPercentage, Float(100) - TestErrorPercentage, TestErrors, State, TaskState);

			if (State != OldState)
			{
				OldState = State;
				SampleRate = Float(0);
			}
		}
		else
		{
			auto info = new dnn::TestingInfo;
			DNNGetTestingInfo(info);

			SampleIndex = info->SampleIndex;
			BatchSize = info->BatchSize;
			Height = info->Height;
			Width = info->Width;
			PadH = info->PadH;
			PadW = info->PadW;
			AvgTestLoss = info->AvgTestLoss;
			TestErrorPercentage = info->TestErrorPercentage;
			TestErrors = info->TestErrors;
			State = safe_cast<dnncore::DNNStates>(info->State);
			TaskState = safe_cast<dnncore::DNNTaskStates>(info->TaskState);
			SampleRate = info->SampleSpeed;

			delete info;

			AdjustedTestingSamplesCount = TestingSamples % BatchSize == 0 ? TestingSamples : ((TestingSamples / BatchSize) + 1) * BatchSize;
			
			TestProgress(BatchSize, SampleIndex, AvgTestLoss, TestErrorPercentage, Float(100) - TestErrorPercentage, TestErrors, State, TaskState);

			if (State != OldState)
			{
				OldState = State;
				SampleRate = Float(0);
			}
		}
	}

	void DNNModel::ResetWeights()
	{
		DNNResetWeights();
	}

	void DNNModel::ResetLayerWeights(UInt layerIndex)
	{
		DNNResetLayerWeights(layerIndex);
	}

	bool DNNModel::SetFormat(bool plain)
	{
		bool ret = DNNSetFormat(plain);

		if (ret)
			PlainFormat = plain;

		return ret;
	}

	void DNNModel::SetOptimizer(DNNOptimizers strategy)
	{
		if (strategy != Optimizer)
		{
			DNNSetOptimizer(safe_cast<dnn::Optimizers>(strategy));
			Optimizer = strategy;
		}
	}

	void DNNModel::SetUseTrainingStrategy(bool enable)
	{
		DNNSetUseTrainingStrategy(enable);
		UseTrainingStrategy = enable;
	}

	void DNNModel::ResetOptimizer()
	{
		DNNResetOptimizer();
	}

	cli::array<String^>^ DNNModel::GetTextLabels(String^ fileName)
	{
		StreamReader^ streamReader;
		String^ str;
		int lines = 0;
		cli::array<String^>^ list;

		try
		{
			streamReader = File::OpenText(fileName);
			while (str = streamReader->ReadLine())
				lines++;
			streamReader->Close();

			list = gcnew cli::array<String^>(lines);
			lines = 0;
			
			streamReader = File::OpenText(fileName);
			while (str = streamReader->ReadLine())
				list[lines++] = gcnew String(str);
			streamReader->Close();
		}
		catch (Exception^)
		{
			/*if (dynamic_cast<FileNotFoundException^>(e))
				Console::WriteLine("file '{0}' not found", fileName);
			else
				Console::WriteLine("problem reading file '{0}'", fileName);*/
			streamReader->Close();
		}
		finally 
		{
			if (streamReader)
				delete (IDisposable^)streamReader;
		}

		return list;
	}

	void DNNModel::SetCostIndex(UInt index)
	{
		DNNSetCostIndex(index);

		CostIndex = index;
		GroupIndex = CostLayers[CostIndex]->GroupIndex;
		LabelIndex = CostLayers[CostIndex]->LabelIndex;
		ClassCount = CostLayers[CostIndex]->ClassCount;
	}

	void DNNModel::UpdateCostInfo(UInt index)
	{
		auto info = new dnn::CostInfo();
		DNNGetCostInfo(index, info);

		CostLayers[index]->TrainErrors = info->TrainErrors;
		CostLayers[index]->TrainLoss = info->TrainLoss;
		CostLayers[index]->AvgTrainLoss = info->AvgTrainLoss;
		CostLayers[index]->TrainErrorPercentage = info->TrainErrorPercentage;
		CostLayers[index]->TrainAccuracy = Float(100) - info->TrainErrorPercentage;

		CostLayers[index]->TestErrors = info->TestErrors;
		CostLayers[index]->TestLoss = info->TestLoss;
		CostLayers[index]->AvgTestLoss = info->AvgTestLoss;
		CostLayers[index]->TestErrorPercentage = info->TestErrorPercentage;
		CostLayers[index]->TestAccuracy = Float(100) - info->TestErrorPercentage;

		delete info;
	}

	void DNNModel::ApplyParameters()
	{
		auto info = new dnn::ModelInfo();
		DNNGetModelInfo(info);

		Name = ToManagedString(info->Name);
		Dataset = safe_cast<DNNDatasets>(info->Dataset);
		CostFunction = safe_cast<DNNCosts>(info->CostFunction);
		LayerCount = info->LayerCount;
		CostLayerCount = info->CostLayerCount;
		CostIndex = info->CostIndex;
		GroupIndex = info->GroupIndex;
		LabelIndex = info->LabelIndex;
		Hierarchies = info->Hierarchies;
		TrainingSamples = info->TrainingSamplesCount;
		TestingSamples = info->TestingSamplesCount;
		MeanStdNormalization = info->MeanStdNormalization;
	
		LabelsCollection = gcnew cli::array<cli::array<String^>^>(int(Hierarchies));

		switch (Dataset)
		{
		case DNNDatasets::tinyimagenet:
			LabelsCollection[0] = GetTextLabels(String::Concat(DatasetsDirectory, Dataset.ToString() + "\\classnames.txt"));
			/*LabelsCollection[0] = gcnew cli::array<String^>(200);
			for (int i = 0; i < 200; i++)
				LabelsCollection[0][i] = i.ToString();*/
			if (info->MeanTrainSet.size() >= 3)
			{
				for (int i = 0; i < 3; i++)
				{
					MeanTrainSet[i] = info->MeanTrainSet[i];
					StdTrainSet[i] = info->StdTrainSet[i];
				}
			}
			break;

		case DNNDatasets::cifar10:
			LabelsCollection[0] = GetTextLabels(String::Concat(DatasetsDirectory, Dataset.ToString() + "\\batches.meta.txt"));
			if (info->MeanTrainSet.size() >= 3)
			{
				for (int i = 0; i < 3; i++)
				{
					MeanTrainSet[i] = info->MeanTrainSet[i];
					StdTrainSet[i] = info->StdTrainSet[i];
				}
			}
			break;

		case DNNDatasets::cifar100:
			LabelsCollection[0] = GetTextLabels(String::Concat(DatasetsDirectory, Dataset.ToString() + "\\coarse_label_names.txt"));
			LabelsCollection[1] = GetTextLabels(String::Concat(DatasetsDirectory, Dataset.ToString() + "\\fine_label_names.txt"));
			if (info->MeanTrainSet.size() >= 3)
			{
				for (int i = 0; i < 3; i++)
				{
					MeanTrainSet[i] = info->MeanTrainSet[i];
					StdTrainSet[i] = info->StdTrainSet[i];
				}
			}
			break;

		case DNNDatasets::fashionmnist:
			LabelsCollection[0] = GetTextLabels(String::Concat(DatasetsDirectory, Dataset.ToString() + "\\batches.meta.txt"));
			if (info->MeanTrainSet.size() >= 1)
			{
				for (int i = 0; i < 1; i++)
				{
					MeanTrainSet[i] = info->MeanTrainSet[i];
					StdTrainSet[i] = info->StdTrainSet[i];
				}
			}
			break;

		case DNNDatasets::mnist:
			LabelsCollection[0] = gcnew cli::array<String^>(10);
			for (int i = 0; i < 10; i++)
				LabelsCollection[0][i] = i.ToString();
			if (info->MeanTrainSet.size() >= 1)
			{
				for (int i = 0; i < 1; i++)
				{
					MeanTrainSet[i] = info->MeanTrainSet[i];
					StdTrainSet[i] = info->StdTrainSet[i];
				}
			}
			break;
		}

		delete info;


		Layers = gcnew System::Collections::ObjectModel::ObservableCollection<DNNLayerInfo^>();
		TrainingStrategies = gcnew System::Collections::ObjectModel::ObservableCollection<DNNTrainingStrategy^>();
		CostLayers = gcnew cli::array<DNNCostLayer^>(int(CostLayerCount));
				
		UInt counter = 0;
		for (UInt layer = 0; layer < LayerCount; layer++)
		{
			Layers->Add(GetLayerInfo(nullptr, layer));

			if (Layers[layer]->LayerType == DNNLayerTypes::Cost)
				CostLayers[counter++] = gcnew DNNCostLayer(Layers[layer]->Cost, Layers[layer]->LayerIndex, Layers[layer]->GroupIndex, Layers[layer]->LabelIndex, Layers[layer]->NeuronCount, Layers[layer]->Name, Layers[layer]->Weight);
		}

		GroupIndex = CostLayers[CostIndex]->GroupIndex;
		LabelIndex = CostLayers[CostIndex]->LabelIndex;
		ClassCount = CostLayers[CostIndex]->ClassCount;

		Optimizer = safe_cast<DNNOptimizers>(GetOptimizer());
	}

	void DNNModel::AddTrainingRate(DNNTrainingRate^ rate, bool clear, UInt gotoEpoch, UInt trainSamples)
	{
		auto nativeRate = dnn::TrainingRate(safe_cast<dnn::Optimizers>(rate->Optimizer), rate->Momentum, rate->Beta2, rate->L2Penalty, rate->Dropout, rate->Eps, rate->BatchSize, rate->Height, rate->Width, rate->PadH, rate->PadW, rate->Cycles, rate->Epochs, rate->EpochMultiplier,rate->MaximumRate, rate->MinimumRate, rate->FinalRate, rate->Gamma, rate->DecayAfterEpochs, rate->DecayFactor, rate->HorizontalFlip, rate->VerticalFlip, rate->InputDropout, rate->Cutout, rate->CutMix, rate->AutoAugment, rate->ColorCast, rate->ColorAngle, rate->Distortion, safe_cast<dnn::Interpolations>(rate->Interpolation), rate->Scaling, rate->Rotation);

		DNNAddTrainingRate(nativeRate, clear, gotoEpoch, trainSamples);
	}

	void DNNModel::AddTrainingRateSGDR(DNNTrainingRate^ rate, bool clear, UInt gotoEpoch, UInt trainSamples)
	{
		auto nativeRate = dnn::TrainingRate(safe_cast<dnn::Optimizers>(rate->Optimizer), rate->Momentum, rate->Beta2, rate->L2Penalty, rate->Dropout, rate->Eps, rate->BatchSize, rate->Height, rate->Width, rate->PadH, rate->PadW, rate->Cycles, rate->Epochs, rate->EpochMultiplier, rate->MaximumRate, rate->MinimumRate, rate->FinalRate, rate->Gamma, rate->DecayAfterEpochs, rate->DecayFactor, rate->HorizontalFlip, rate->VerticalFlip, rate->InputDropout, rate->Cutout, rate->CutMix, rate->AutoAugment, rate->ColorCast, rate->ColorAngle, rate->Distortion, safe_cast<dnn::Interpolations>(rate->Interpolation), rate->Scaling, rate->Rotation);

		DNNAddTrainingRateSGDR(nativeRate, clear, gotoEpoch, trainSamples);
	}

	void DNNModel::ClearTrainingStrategies()
	{
		DNNClearTrainingStrategies();
	}

	void DNNModel::AddTrainingStrategy(DNNTrainingStrategy^ strategy)
	{
		DNNAddTrainingStrategy(dnn::TrainingStrategy(strategy->Epochs, strategy->BatchSize, strategy->Height, strategy->Width, strategy->PadH, strategy->PadW, strategy->Momentum, strategy->Beta2, strategy->Gamma, strategy->L2Penalty, strategy->Dropout, strategy->HorizontalFlip, strategy->VerticalFlip, strategy->InputDropout, strategy->Cutout, strategy->CutMix, strategy->AutoAugment, strategy->ColorCast, strategy->ColorAngle, strategy->Distortion, safe_cast<dnn::Interpolations>(strategy->Interpolation), strategy->Scaling, strategy->Rotation));
	}

	void DNNModel::Start(bool training)
	{
		if (NewEpoch != nullptr)
			DNNSetNewEpochDelegate((void(*)(UInt, UInt, UInt, UInt, Float, Float, Float, bool, bool, Float, Float, bool, Float, Float, UInt, Float, UInt, Float, Float, Float, UInt, UInt, UInt, Float, Float, Float, Float, Float, Float, UInt, Float, Float, Float, UInt))(Marshal::GetFunctionPointerForDelegate(NewEpoch).ToPointer()));
		SampleRate = Float(0);
		State = DNNStates::Idle;

		IsTraining = training;
		if (IsTraining)
			DNNTraining();
		else 
			DNNTesting();

		TaskState = DNNTaskStates::Running;
		WorkerTimer->Start();
		Duration->Start();
	}

	void DNNModel::Stop()
	{
		SampleRate = Float(0);
		Duration->Reset();
		DNNStop();
		WorkerTimer->Stop();
		State = DNNStates::Completed;
		TaskState = DNNTaskStates::Stopped;
	}

	void DNNModel::Pause()
	{
		WorkerTimer->Stop();
		Duration->Stop();
		DNNPause();
		TaskState = DNNTaskStates::Paused;
	}

	void DNNModel::Resume()
	{
		DNNResume();
		Duration->Start();
		WorkerTimer->Start();
		TaskState = DNNTaskStates::Running;
	}

	DNNCheckMsg^ DNNModel::Check(String^ definition)
	{
		dnn::CheckMsg checkMsg;
		
		auto def = ToUnmanagedString(definition);
		DNNCheck(def, checkMsg);
				
		definition = ToManagedString(def);

		return gcnew DNNCheckMsg(checkMsg.Row, checkMsg.Column, ToManagedString(checkMsg.Message), checkMsg.Error, definition);
	}

	int DNNModel::Load(String^ fileName)
	{
		dnn::CheckMsg checkMsg;

		DNNModelDispose();
		DNNDataprovider(ToUnmanagedString(StorageDirectory));

		GC::Collect(GC::MaxGeneration, GCCollectionMode::Forced, true, true);

		if (DNNLoad(ToUnmanagedString(fileName), checkMsg))
		{
			DNNLoadDataset();

			auto reader = gcnew System::IO::StreamReader(fileName, true);
			Definition = reader->ReadToEnd();
			reader->Close();

			DNNResetWeights();
			ApplyParameters();
		}
		else
			throw std::exception(checkMsg.Message.c_str());

		GC::Collect(GC::MaxGeneration, GCCollectionMode::Forced, true, true);

		return 1;
	}

	void DNNModel::SetLocked(bool locked)
	{
		DNNSetLocked(locked);
		for (int i = 0; i < LayerCount; i++)
			if (Layers[i]->Lockable)
				Layers[i]->LockUpdate = locked;
	}

	void DNNModel::SetLayerLocked(UInt layerIndex, bool locked)
	{
		DNNSetLayerLocked(layerIndex, locked);
	}

	int DNNModel::LoadWeights(String^ fileName, bool persist)
	{
		int ret = DNNLoadWeights(ToUnmanagedString(fileName), persist);

		Optimizer = safe_cast<DNNOptimizers>(GetOptimizer());

		if (ret == 0 && SelectedIndex > 0)
			UpdateLayerStatistics(Layers[SelectedIndex], SelectedIndex, true);

		return ret;
	}

	int DNNModel::SaveWeights(String^ fileName, bool persist)
	{
		return DNNSaveWeights(ToUnmanagedString(fileName), persist);
	}

	int DNNModel::LoadLayerWeights(String^ fileName, UInt layerIndex)
	{
		int ret = DNNLoadLayerWeights(ToUnmanagedString(fileName), layerIndex, false);

		if (ret == 0 && SelectedIndex > 0)
			UpdateLayerStatistics(Layers[layerIndex], layerIndex, layerIndex == SelectedIndex);

		return ret;
	}

	int DNNModel::SaveLayerWeights(String^ fileName, UInt layerIndex)
	{
		return DNNSaveLayerWeights(ToUnmanagedString(fileName), layerIndex, false);
	}

	bool DNNModel::StochasticEnabled()
	{
		return DNNStochasticEnabled();
	}
}