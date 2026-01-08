using System;
using NeuralNetwork1;

namespace NeuralNetworkZodiac
{
    public delegate void TrainProgressHandler(double progress, double error, TimeSpan time);

    public abstract class BaseNetwork
    {
        public event TrainProgressHandler TrainProgress;

        public abstract int Train(Sample sample, double acceptableError, bool parallel);
        public abstract double TrainOnDataSet(SamplesSet samplesSet, int epochsCount, double acceptableError, bool parallel);
        protected abstract double[] Compute(double[] input);

        public ZodiacSign Predict(Sample sample)
        {
            return sample.ProcessPrediction(Compute(sample.input));
        }

        protected virtual void OnTrainProgress(double progress, double error, TimeSpan time)
        {
            TrainProgress?.Invoke(progress, error, time);
        }
    }
}