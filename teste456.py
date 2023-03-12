import os
import azure.cognitiveservices.speech as speechsdk
# This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
speech_config = speechsdk.SpeechConfig(subscription='f837b896af1646f58390f977edcfeed4', region='northeurope')
done = False
# configuração do audio
audio_config = speechsdk.audio.AudioConfig(use_default_microphone=True)

auto_detect_source_language_config = \
    speechsdk.languageconfig.AutoDetectSourceLanguageConfig(languages=["en-US", "pt-PT"])
speech_recognizer = speechsdk.SpeechRecognizer(
    speech_config=speech_config,
    auto_detect_source_language_config=auto_detect_source_language_config, 
    audio_config=audio_config)

myDictonary = {
    'pt-PT': 'pt-PT-DuarteNeural',
    'en-US': 'en-US-BrandonNeural'
}
teste = []
while not done:
    print("Speak into your microphone.")
    speech_recognition_result = speech_recognizer.recognize_once()
    auto_detect_source_language_result = speechsdk.AutoDetectSourceLanguageResult(speech_recognition_result)
    detected_language = auto_detect_source_language_result.language
    if speech_recognition_result.reason == speechsdk.ResultReason.RecognizedSpeech:
        if speech_recognition_result.text == "Sair.":
            for a in teste:
                print(a)
                speech_synthesizer.speak_text_async(a).get()
            done = True
        else:
            speech_config.speech_synthesis_voice_name=myDictonary[detected_language]
            speech_synthesizer = speechsdk.SpeechSynthesizer(speech_config=speech_config, audio_config=audio_config)
            teste.append(speech_recognition_result.text)
            print("Recognized: {}".format(speech_recognition_result.text))
            text = speech_recognition_result.text
            speech_synthesis_result = speech_synthesizer.speak_text_async(text).get()
   
    pass