#pragma once
#include "Dataprovider.h"
#include "Layer.h"
#include "Activation.h"
#include "Add.h"
#include "Average.h"
#include "AvgPooling.h"
#include "BatchNorm.h"
#include "BatchNormActivation.h"
#include "BatchNormActivationDropout.h"
#include "BatchNormRelu.h"
#include "ChannelMultiply.h"
#include "ChannelShuffle.h"
#include "ChannelSplit.h"
#include "ChannelZeroPad.h"
#include "Concat.h"
#include "Convolution.h"
#include "ConvolutionTranspose.h"
#include "Cost.h"
#include "Dense.h"
#include "DepthwiseConvolution.h"
#include "Divide.h"
#include "Dropout.h"
#include "GlobalAvgPooling.h"
#include "GlobalMaxPooling.h"
#include "Input.h"
#include "LocalResponseNormalization.h"
#include "Max.h"
#include "MaxPooling.h"
#include "Min.h"
#include "Multiply.h"
#include "PartialDepthwiseConvolution.h"
#include "Substract.h"
#include "Resampling.h"


namespace dnn
{
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

	class Model
	{
	public:
		const std::string Name;
		const dnnl::engine Engine;
		dnn::Device Device;
		dnnl::memory::format_tag Format;
		Dataprovider* DataProv;
		Datasets Dataset;
		std::atomic<States> State;
		std::atomic<TaskStates> TaskState;
		Costs CostFuction;
		Optimizers Optimizer;
		size_t CostIndex;
		size_t LabelIndex;
		size_t GroupIndex;
		size_t TotalCycles;
		size_t TotalEpochs;
		size_t CurrentCycle;
		size_t CurrentEpoch;
		size_t SampleIndex;
		//size_t LogInterval;
		size_t BatchSize;
		size_t GoToEpoch;
		size_t AdjustedTrainingSamplesCount;
		size_t AdjustedTestingSamplesCount;
		size_t TrainSkipCount;
		size_t TestSkipCount;
		size_t TrainOverflowCount;
		size_t TestOverflowCount;
		size_t SampleC;
		size_t SampleD;
		size_t SampleH;
		size_t SampleW;
		size_t PadD;
		size_t PadH;
		size_t PadW;
		bool MirrorPad;
		bool RandomCrop;
		bool MeanStdNormalization;
		Fillers WeightsFiller;
		Float WeightsScale;
		Float WeightsLRM;
		Float WeightsWDM;
		Fillers BiasesFiller;
		Float BiasesScale;
		Float BiasesLRM;
		Float BiasesWDM;
		Float AlphaFiller;
		Float BetaFiller;
		Float BatchNormMomentum;
		Float BatchNormEps;
		Float Dropout;
		size_t TrainErrors;
		size_t TestErrors;
		Float TrainLoss;
		Float TestLoss;
		Float AvgTrainLoss;
		Float AvgTestLoss;
		Float TrainErrorPercentage;
		Float TestErrorPercentage;
		Float Accuracy;
		Float Rate;
		bool BatchNormScaling;
		bool HasBias;
		bool PersistOptimizer;
		bool DisableLocking;
		TrainingRate CurrentTrainingRate;
		std::vector<Layer*> Layers;
		std::vector<Cost*> CostLayers;
		std::vector<TrainingRate> TrainingRates;
		std::chrono::duration<Float> fpropTime;
		std::chrono::duration<Float> bpropTime;
		std::chrono::duration<Float> updateTime;
		std::atomic<size_t> FirstUnlockedLayer;
		std::atomic<bool> BatchSizeChanging;
		std::atomic<bool> ResettingWeights;

		Model(const char* name, Dataprovider* dataprovider);
		~Model();

		void(*NewEpoch)(size_t, size_t, size_t, bool, bool, Float, Float, Float, Float, size_t, Float, size_t, Float, Float, Float, size_t, Float, Float, Float, Float, Float, size_t, Float, Float, Float, size_t);
		
		static bool CheckDefinition(std::string& definition, CheckMsg& checkMsg);
		static Model* ReadDefinition(const char* definition, const Optimizers optimizer, Dataprovider* dataprovider, CheckMsg& checkMsg);
		static Model* LoadDefinition(const char* fileName, const Optimizers optimizer, Dataprovider* dataprovider, CheckMsg& checkMsg);
		
		std::vector<Layer*> GetLayerInputs(const std::vector<std::string>& inputs) const;
		std::vector<Layer*> GetLayerOutputs(const Layer* layer) const;
		std::vector<Layer*> SetRelations();
		bool GetInputSnapShot(std::vector<Float>* snapshot, std::vector<size_t>* label);
#ifdef DNN_STOCHASTIC
		void CostFunction(const States state);
		void Recognized(const States state, const std::vector<size_t>& sampleLabel);
#endif
		void CostFunctionBatch(const States state, const size_t batchSize, const bool overflow, const size_t skipCount);
		void RecognizedBatch(const States state, const size_t batchSize, const bool overflow, const size_t skipCount, const std::vector<std::vector<size_t>>& sampleLabels);
		void SetOptimizer(const Optimizers optimizer);
		void SetLocking(const bool locked);
		void SetLayerLocking(const size_t layerIndex, const bool locked);
		void Training();
		void Testing();
		void TrainingAsync();
		void TestingAsync();
		void StopTask();
		void PauseTask();
		void ResumeTask();
		bool CheckTaskState() const;
		bool IsUniqueLayerName(const char* name) const;
		void SetHyperParameters(const Float adaDeltaEps, const Float adaGradEps, const Float adamEps, const Float adamBeta2, const Float adamaxEps, const Float adamaxBeta2, const Float rmsPropEps, const Float radamEps, const Float radamBeta1, const Float radamBeta2);
		void AddTrainingRate(const TrainingRate rate, const bool clear = false, const size_t gotoEpoch = 1);
		void AddTrainingRateSGDR(const TrainingRate rate, const bool clear = false, const size_t gotoEpoch = 1);
		void ResetWeights();
		void SetBatchSize(const size_t batchSize);
		bool BatchNormalizationUsed() const;
		size_t GetWeightsSize(const bool persistOptimizer = false) const;
		size_t GetNeuronsSize(const size_t batchSize) const;
		
		void ForwardProp(const size_t batchSize);
		void BackwardProp(const size_t batchSize);
		
#ifdef DNN_STOCHASTIC
		std::vector<size_t> TrainSample(const size_t index);
		std::vector<size_t> TestSample(const size_t index);
		std::vector<size_t> TestAugmentedSample(const size_t index);
#endif
		std::vector<std::vector<size_t>> TrainBatch(const size_t index, const size_t batchSize);
		std::vector<std::vector<size_t>> TestBatch(const size_t index, const size_t batchSize);
		std::vector<std::vector<size_t>> TestAugmentedBatch(const size_t index, const size_t batchSize);

		int SaveWeights(const char* fileName, const bool persistOptimizer = false) const
		{
			auto os = std::ofstream(fileName, std::ios::out | std::ios::binary | std::ios::trunc);

			if (!os.bad() && os.is_open())
			{
				for (auto layer : Layers)
					layer->Save(os, persistOptimizer, Optimizer);

				os.close();

				return 0;
			}

			return -1;
		}

		int LoadWeights(const char* fileName, const bool persistOptimizer = false)
		{
			if (GetFileSize(fileName) == GetWeightsSize(persistOptimizer))
			{
				auto is = std::ifstream(fileName, std::ios::in | std::ios::binary);

				if (!is.bad() && is.is_open())
				{
					for (auto layer : Layers)
						layer->Load(is, persistOptimizer, Optimizer);

					is.close();

					return 0;
				}
			}

			return -1;
		}

		int SaveLayerWeights(const char* fileName, const size_t layerIndex, const bool persistOptimizer = false) const
		{
			auto os = std::ofstream(fileName, std::ios::out | std::ios::binary | std::ios::trunc);

			if (!os.bad() && os.is_open())
			{
				Layers[layerIndex]->Save(os, persistOptimizer, Optimizer);

				os.close();

				return 0;
			}

			return -1;
		}

		int LoadLayerWeights(const char* fileName, const size_t layerIndex, const bool persistOptimizer = false)
		{
			if (GetFileSize(fileName) == Layers[layerIndex]->GetWeightsSize(persistOptimizer, Optimizer))
			{
				auto is = std::ifstream(fileName, std::ios::in | std::ios::binary);

				if (!is.bad() && is.is_open())
				{
					Layers[layerIndex]->Load(is, persistOptimizer, Optimizer);

					is.close();

					return 0;
				}
			}

			return -1;
		}
				
		private:
		   	std::future<void> task;
			std::vector<size_t> RandomTrainingSamples;
			std::vector<bool> TrainingSamplesHFlip;
			std::vector<bool> TrainingSamplesVFlip;
			std::vector<bool> TestingSamplesHFlip;
			std::vector<bool> TestingSamplesVFlip;
	};
}