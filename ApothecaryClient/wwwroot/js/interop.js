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
    }
};
