# HelpChat Lab Files

### NOTE: The implementation of Phi Silica has changed and you must complete these additional steps to make the application run correctly: https://learn.microsoft.com/en-us/windows/ai/apis/get-started?tabs=winget%2Cwinui%2Cwinui2 (look for the steps starting with "Edit the Package.appxmanifest file"). You may also need to change the implementation in PhiSilicaClient.cs.

👉 The PowerPoint presentation, the Jupyter notebook, and code snippets are in the `_GOODIES INSIDE` folder.

👉 You must make sure that your device is capable of running Phi Silica (Copilot+ PC with NPU, Windows Insider beta or higher). Be sure you can run a Phi Silica sample from the AI Dev Gallery first. https://learn.microsoft.com/en-us/windows/ai/ai-dev-gallery/

> **_NOTE:_** This repo does NOT include the ONNX model because it's too big.
> - Download `model.onnx` from https://huggingface.co/protectai/deberta-v3-large-zeroshot-v1-onnx/tree/main.
> - Rename that file to `zero_shot_classifier.onnx` file, drop it in the AIModels folder (alongside `Labels.cs` and `vocab.txt`)
> - If required, set its **Build Action** to `Content` and **Copy to Output Directory** to `Copy if newer`

The Azure resource URLs/keys in App.xaml will need to be replaced, as they are no longer functional after the lab.


## License

This project is licensed under the [MIT License](LICENSE). Please provide attribution.
