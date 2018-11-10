using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Models;
using Microsoft.ML.Legacy.Data;
using Microsoft.ML.Legacy.Transforms;
using Microsoft.ML.Legacy.Trainers;
using Microsoft.ML.Runtime.Api;
using SentimentAnalysis.Models;

namespace SentimentAnalysis
{
    class Program
    {
        static readonly string _dataPositivePath = Path.Combine(Environment.CurrentDirectory, "data/train", "pos");
        static readonly string _dataNegativePath = Path.Combine(Environment.CurrentDirectory, "data/train", "neg");

        static readonly string _testDataPath = Path.Combine(Environment.CurrentDirectory, "imbd-sentiment.txt");
        static readonly string _modelpath = Path.Combine(Environment.CurrentDirectory, "data", "Model.zip");

        static async Task Main(string[] args)
        {
            PrepareData();

            var model = await Train();

            Evaluate(model);
            Predict(model);
        }


        private static void PrepareData()
        {
            // this is designed to work on the IMDB dataset available @
            // http://ai.stanford.edu/~amaas/data/sentiment/
            // extract this file and change this path to point to the local folder for training
                
            var positiveReviewsDirectory = new DirectoryInfo(_dataPositivePath);
            var negativeReviewsDirectory = new DirectoryInfo(_dataNegativePath);

            var outFile = "imbd-sentiment.txt";
            using (var logWriter = new StreamWriter(outFile, false))
            {
                foreach (var positiveReview in positiveReviewsDirectory.GetFiles())
                {
                    logWriter.WriteLine($"1{"\t"}{File.ReadAllText(positiveReview.FullName)}");
                }
                foreach (var negativeReview in negativeReviewsDirectory.GetFiles())
                {
                    logWriter.WriteLine($"0{"\t"}{File.ReadAllText(negativeReview.FullName)}");
                }
            }
        }

        public static async Task<PredictionModel<SentimentDataModel, SentimentPredictionModel>> Train()
        {
            // LearningPipeline allows you to add steps in order to keep everything together 
            // during the learning process.  

            var pipeline = new LearningPipeline();

            // The TextLoader loads a dataset with comments and corresponding postive or negative sentiment. 
            // When you create a loader, you specify the schema by passing a class to the loader containing
            // all the column names and their types. This is used to create the model, and train it. 
            pipeline.Add(new TextLoader(Path.Combine(Environment.CurrentDirectory, "imbd-sentiment.txt")).CreateFrom<SentimentDataModel>());

            // TextFeaturizer is a transform that is used to featurize an input column. 
            // This is used to format and clean the data.
            pipeline.Add(new TextFeaturizer("Features", "SentimentText"));

            // Adds a FastTreeBinaryClassifier, the decision tree learner for this project, and 
            // three hyperparameters to be used for tuning decision tree performance.
            pipeline.Add(new FastTreeBinaryClassifier() { NumLeaves = 5, NumTrees = 5, MinDocumentsInLeafs = 2 });


            // Train the pipeline based on the dataset that has been loaded, transformed.
            PredictionModel<SentimentDataModel, SentimentPredictionModel> model =
                pipeline.Train<SentimentDataModel, SentimentPredictionModel>();

            // Saves the model we trained to a zip file.
            await model.WriteAsync(_modelpath);

            // Returns the model we trained to use for evaluation.
            return model;
        }

        public static void Evaluate(PredictionModel<SentimentDataModel, SentimentPredictionModel> model)
        {
            // Evaluates.
            var testData = new TextLoader(_testDataPath).CreateFrom<SentimentDataModel>();

            // BinaryClassificationEvaluator computes the quality metrics for the PredictionModel
            // using the specified data set.
            var evaluator = new BinaryClassificationEvaluator();

            // BinaryClassificationMetrics contains the overall metrics computed by binary classification evaluators.
            BinaryClassificationMetrics metrics = evaluator.Evaluate(model, testData);

            // The Accuracy metric gets the accuracy of a classifier, which is the proportion 
            // of correct predictions in the test set.

            // The Auc metric gets the area under the ROC curve.
            // The area under the ROC curve is equal to the probability that the classifier ranks
            // a randomly chosen positive instance higher than a randomly chosen negative one
            // (assuming 'positive' ranks higher than 'negative').

            // The F1Score metric gets the classifier's F1 score.
            // The F1 score is the harmonic mean of precision and recall:
            //  2 * precision * recall / (precision + recall).

            Console.WriteLine();
            Console.WriteLine("PredictionModel quality metrics evaluation");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"Auc: {metrics.Auc:P2}");
            Console.WriteLine($"F1Score: {metrics.F1Score:P2}");
        }

        public static void Predict(PredictionModel<SentimentDataModel, SentimentPredictionModel> model)
        {
            // Adds some comments to test the trained model's predictions.
            IEnumerable<SentimentDataModel> sentiments = new[]
            {
                new SentimentDataModel
                {
                    SentimentText = "Please refrain from adding nonsense to Wikipedia."
                },
                new SentimentDataModel
                {
                    SentimentText = "He is the best, and the article should say that."
                }
            };

            // Use the model to predict the positive 
            // or negative sentiment of the comment data.
            IEnumerable<SentimentPredictionModel> predictions = model.Predict(sentiments);

            Console.WriteLine();
            Console.WriteLine("Sentiment Predictions");
            Console.WriteLine("---------------------");

            // Builds pairs of (sentiment, prediction)
            var sentimentsAndPredictions = sentiments.Zip(predictions, (sentiment, prediction) => (sentiment, prediction));

            foreach (var item in sentimentsAndPredictions)
            {
                Console.WriteLine($"Sentiment: {item.sentiment.SentimentText} | Prediction: {(item.prediction.Sentiment ? "Negative" : "Positive")}");
            }
            Console.WriteLine();     
        }
    }
}