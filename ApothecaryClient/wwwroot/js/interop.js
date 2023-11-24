window.interop = {
    readFileAsBase64: function (inputElement) {
        return new Promise((resolve, reject) => {
            const file = inputElement.files[0];
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result.split(',')[1]);
            reader.onerror = () => reject('Error reading file');
            reader.readAsDataURL(file);
        });
    },
    showImagePreview: function (element, dotNetReference) {
        if (element.files && element.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                dotNetReference.invokeMethodAsync('SetImagePreview', e.target.result);
            };
            reader.readAsDataURL(element.files[0]);
        }
    },

    monitorPaymentStatus: function (dotNetReference) {
        var checkPaymentStatus = setInterval(function () {
            var paymentButton = document.querySelector('.eway-button.processed');
            if (paymentButton) {
                clearInterval(checkPaymentStatus);
                var receiptNumber = document.querySelector('.right-tip').getAttribute('data-tips').match(/Receipt Number:(\d+)/)[1];
                dotNetReference.invokeMethodAsync('PaymentCompleted', receiptNumber);
            }
        }, 500); // Check every 500 milliseconds
    },
    mediaRecorder: null,
    audioChunks: [],

    startRecording: function () {
        navigator.mediaDevices.getUserMedia({ audio: true })
            .then(stream => {
                this.mediaRecorder = new MediaRecorder(stream);
                this.mediaRecorder.start();

                this.audioChunks = [];
                this.mediaRecorder.addEventListener("dataavailable", event => {
                    this.audioChunks.push(event.data);
                });
            });
    },

    stopRecording: function () {
        return new Promise(resolve => {
            this.mediaRecorder.addEventListener("stop", () => {
                const audioBlob = new Blob(this.audioChunks);
                const reader = new FileReader();
                reader.readAsDataURL(audioBlob);
                reader.onloadend = () => {
                    const base64String = reader.result;
                    resolve(base64String.split(',')[1]); // return only the base64 content, not the entire data URL
                };
            });

            this.mediaRecorder.stop();
        });
    },

    playRecording: function (base64Audio) {
        const audioUrl = "data:audio/wav;base64," + base64Audio;
        const audio = new Audio(audioUrl);
        audio.play();
    }
};
