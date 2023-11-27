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
    },
    updatePaymentAmount: function (amountInCents) {
        var paymentScript = document.querySelector('.eway-paynow-button');
        if (paymentScript) {
            paymentScript.setAttribute('data-amount', amountInCents.toString());
        }
    },
    updatePaymentButtonStyle: function () {
        var paymentButton = document.querySelector('.eway-button');
        if (paymentButton) {
            // Change background color to green
            paymentButton.style.background = '#32CD32!important'; // Example green color, adjust as needed

            // Apply neumorphic styles
            paymentButton.style.boxShadow = '8px 8px 15px #a7a7a7, -8px -8px 15px #ffffff';
            paymentButton.style.borderRadius = '10px';
            paymentButton.style.textShadow = 'none';
            paymentButton.style.color = '#000000'; // Adjust text color as needed
        }
    },

    SendMessage: function () {
        var userMessage = document.getElementById("userMessage").value;
        var apiUrl = "https://localhost:7092/api/chat/GetOpenAIResponse?usermessage=" + encodeURIComponent(userMessage);
        var typingAnimation = document.getElementById("typingAnimation");
        typingAnimation.style.display = "inline-block";

        document.getElementById("userMessage").value = "";
        document.getElementById("userMessage").placeholder = "Chat with Elisa";

        fetch(apiUrl, {
            method: "POST"
        })
            .then(function (response) {
                return response.json();
            })
            // Inside the JavaScript function where you handle message sending
            .then(function (data) {
                typingAnimation.style.display = "none";
                var chatTable = document.getElementById("chatTable");

                // Create a new table row for the user
                var userTableRow = document.createElement("tr");                

                // Create a cell for user bubble
                var userBubbleCell = document.createElement("td");
                userBubbleCell.className = "chat-bubble user-bubble";

                // Create a span element for user message
                var userMessageSpan = document.createElement("span");
                userMessageSpan.textContent = "User: " + userMessage;

                // Append the user message to the user bubble cell
                userBubbleCell.appendChild(userMessageSpan);

                // Append the user bubble cell to the user table row
                userTableRow.appendChild(userBubbleCell);

                // Append the user row to the table
                chatTable.appendChild(userTableRow);

                // Create a cell for the user's avatar
                var userAvatarCell = document.createElement("td");
                userAvatarCell.className = "avatar-cell";

                // Create an image element for the user's avatar
                var userAvatarImage = document.createElement("img");
                userAvatarImage.src = "/images/user.png"; // Replace with the actual URL of the user's avatar
                userAvatarImage.alt = "User Avatar";

                // Append the user's avatar image to the cell
                userAvatarCell.appendChild(userAvatarImage);

                // Append the user's avatar cell to the user table row
                userTableRow.appendChild(userAvatarCell);

                // Create a new table row for Elisa AI
                var elisaTableRow = document.createElement("tr");

                // Create a cell for Elisa AI's avatar
                var elisaAvatarCell = document.createElement("td");
                elisaAvatarCell.className = "avatar-cell";

                // Create an image element for Elisa AI's avatar
                var elisaAvatarImage = document.createElement("img");
                elisaAvatarImage.src = "/images/robot.png"; // Replace with the actual URL of Elisa AI's avatar
                elisaAvatarImage.alt = "Elisa AI Avatar";

                // Append Elisa AI's avatar image to the cell
                elisaAvatarCell.appendChild(elisaAvatarImage);

                // Append the Elisa AI's avatar cell to the Elisa AI table row
                elisaTableRow.appendChild(elisaAvatarCell);

                // Create a cell for Elisa AI bubble
                var elisaBubbleCell = document.createElement("td");
                elisaBubbleCell.className = "chat-bubble elisa-bubble";

                // Create a span element for Elisa AI's message
                var elisaMessageSpan = document.createElement("span");
                elisaMessageSpan.textContent = "Elisa AI: " + data.response;

                // Append Elisa AI's message to the Elisa bubble cell
                elisaBubbleCell.appendChild(elisaMessageSpan);

                // Append the Elisa bubble cell to the Elisa AI table row
                elisaTableRow.appendChild(elisaBubbleCell);

                // Append the Elisa AI row to the table
                chatTable.appendChild(elisaTableRow);
            })

    },
    scrollToOrderPlacement: function () {
        const orderPlacementSection = document.querySelector('.order-placement-container');
        if (orderPlacementSection) {
            orderPlacementSection.scrollIntoView({ behavior: 'smooth' });
        }
    }



};
