module Analysis

open System
open System.IO
open Microsoft.ML
open Microsoft.ML.Data

type SentimentData () =
    [<DefaultValue>]
    [<LoadColumn(0)>]
    val mutable public SentimentText :string

    [<DefaultValue>]
    [<LoadColumn(1)>]
    val mutable public Label :bool
    
    // NOTE: Need to add this column to extract metrics
    [<DefaultValue>]
    [<LoadColumn(2)>]
    val mutable public Probability :float32

type SentimentPrediction () =
    [<DefaultValue>]
    val mutable public SentimentData :string

    [<DefaultValue>]
    val mutable public PredictedLabel :bool

    [<DefaultValue>]
    val mutable public Score :float32 


module ML =
    let initPredictor (dataFile:string) =
        let ml = new MLContext()
        let reader = ml.Data.CreateTextReader<SentimentData>(separatorChar = '\t', hasHeader = true)
      
        let allData = reader.Read(dataFile);
        let struct (trainData, testData) = ml.Clustering.TrainTestSplit(allData, testFraction = 0.3)
    
        let reader = 
          ml.Data.CreateTextReader(
            separatorChar = '\t',
            hasHeader = true,
            columns = 
              [|
                  Data.TextLoader.Column("SentimentText", Nullable Data.DataKind.Text, 0);
                  Data.TextLoader.Column("Label", Nullable Data.DataKind.Bool, 1);                  
                  Data.TextLoader.Column("Probability", Nullable Data.DataKind.R4, 2)
              |])
    
        let pipeline = 
          ml
            .Transforms.Text.FeaturizeText("SentimentText", "Features")
            .Append(ml.BinaryClassification.Trainers.FastForest())
            .Append(ml.BinaryClassification.Trainers.FastForest(numTrees = 500, numLeaves = 100, learningRate = 0.0001))
        let model = pipeline.Fit(trainData)

        model.CreatePredictionEngine<SentimentData, SentimentPrediction>(ml)
    
    let saveModel (ml:MLContext) model (path:string) =
      use fsWrite = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write)
      ml.Model.Save(model, fsWrite)

    
    // Load model from file
    let loadModel (path:string) =
        
        use fsRead = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
        let mlReloaded = MLContext()
        let modelReloaded = TransformerChain.LoadFrom(mlReloaded, fsRead)
        modelReloaded.CreatePredictionEngine<SentimentData, SentimentPrediction>(mlReloaded)

    let runPrediction (model:PredictionEngine<SentimentData, SentimentPrediction>) (testText:string) =
        let test = SentimentData()
        test.SentimentText <- testText
        model.Predict(test)
        
    let scorePrediction (prediction:SentimentPrediction) = 
        printfn "score : %O"  prediction.Score
        prediction.Score