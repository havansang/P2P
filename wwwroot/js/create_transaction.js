
// Định dạng tiền VND
function formatMoney(event) {
    const $input = $(event.target);
    let value = $input.val().replace(/\D/g, '');

    if (value === '') {
        $input.val('');
        $('#AmountInput').val('');
        return;
    }

    const formattedValue = value.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    $input.val(formattedValue);

    const cleanValue = formattedValue.replace(/\./g, '');
    $('#AmountInput').val(cleanValue);
}

// Kiểm tra định dạng email
function isValidEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}

// Hiển thị Toast thông báo
function showToast(message, type = "success") {
    const toastId = "toast-" + Date.now();

    let bgColor = "bg-success";
    let iconHtml = '<i class="fas fa-check-circle me-2 text-white"></i>';

    if (type === "error" || type === "danger") {
        bgColor = "bg-danger";
        iconHtml = '<i class="fas fa-exclamation-triangle me-2 text-warning"></i>';
    }

    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white ${bgColor} border-0 show mb-2 shadow" role="alert">
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center">
                    ${iconHtml}
                    <span>${message}</span>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>`;

    $("#toast-container").append(toastHtml);

    setTimeout(() => {
        $(`#${toastId}`).fadeOut(300, function () {
            $(this).remove();
        });
    }, 3000);
}

// Sự kiện kiểm tra email người nhận
$('#ReceiverEmail').on('input', function () {
    const email = $(this).val().trim();
    const $result = $('#emailCheckResult');

    if (email === '') {
        $result.html('');
        return;
    }

    if (!isValidEmail(email)) {
        $result.html('<span class="text-danger">❌ Địa chỉ email không hợp lệ.</span>');
        return;
    }

    $.ajax({
        url: '/Transaction/CheckReceiverEmail',
        type: 'POST',
        data: { email: email },
        success: function (response) {
            if (response.exists) {
                $result.html('<span class="text-success">✅ Tài khoản người nhận hợp lệ.</span>');
                $('#previewBtn').prop('disabled', false);
            } else {
                $result.html('<span class="text-warning">⚠️ Email này chưa đăng ký tài khoản trên hệ thống.</span>');
                $('#previewBtn').prop('disabled', true);

            }
        },
        error: function () {
            $result.html('<span class="text-danger">⚠️ Không thể kiểm tra tài khoản. Vui lòng thử lại.</span>');
            $('#previewBtn').prop('disabled', true);

        }
    });
});

// Chuyển đổi giữa Form và Xác nhận
$(document).ready(function () {
    const $formSection = $('#formSection');
    const $confirmSection = $('#confirmationSection');

    $('#previewBtn').on('click', function () {
        const amountt = $('input[name="Amount"]').val();
        const receiverEmail = $('input[name="ReceiverEmail"]').val();
        /*const feePercent = parseInt($('#FeePercent').val()) || 0;*/
        const feePercent = window.feePercent;
        const mail = window.mail;
        const amount = amountt * (1 + feePercent / 100);
        const desc = $('textarea[name="Description"]').val();
        const method = $('input[name="PaymentMethod"]:checked').next().text();

        if (!amount || !desc || receiverEmail=="") {
            showToast("Vui lòng điền đầy đủ thông tin.", "error");
            return;
        }
        if (!isValidEmail(receiverEmail)) {
            showToast("Địa chỉ email không hợp lệ.", "error");
            return;
        }
        if (receiverEmail == mail) {
            showToast("Vui lòng điền email khác email của bạn!", "error");
            return;
        }

        $('#confirmAmount').text(`${parseInt(amount).toLocaleString()} VNĐ`);
        $('#cofirmReceiverEmail').text(receiverEmail);
        $('#confirmDescription').text(desc);
        $('#confirmPayment').text(method);

        $formSection.removeClass('fade-in').addClass('fade-out');
        setTimeout(() => {
            $formSection.addClass('hidden-section').removeClass('fade-out');
            $confirmSection.removeClass('hidden-section fade-out').addClass('fade-in');
        }, 500);
    });

    $('#backBtn').on('click', function () {
        $confirmSection.removeClass('fade-in').addClass('fade-out');
        setTimeout(() => {
            $confirmSection.addClass('hidden-section').removeClass('fade-out');
            $formSection.removeClass('hidden-section fade-out').addClass('fade-in');
        }, 500);
    });
});

// Hiển thị QR và xử lý giao dịch
$('#confirmBtn').click(function () {
    const amountt = $('input[name="Amount"]').val();
    const accountId = window.accountId;
    const feePercent = window.feePercent;
    const amount = amountt * (1 + feePercent / 100);
    const now = new Date();
    const timestamp = `${now.getHours()}${now.getMinutes()}${now.getSeconds()}`;
    const description = `${accountId}${timestamp}`;

    const qrImageUrl = `https://img.vietqr.io/image/MB-0868090203-compact2.png?amount=${amount}&addInfo=${description}&accountName=HA VAN SANG`;
    $('#qrImage').attr('src', qrImageUrl);

    const qrModal = new bootstrap.Modal(document.getElementById('qrPopup'));
    qrModal.show();

    let timeLeft = 120;
    const countdownEl = $('#countdown');

    const countdownInterval = setInterval(() => {
        const minutes = Math.floor(timeLeft / 60);
        const seconds = timeLeft % 60;
        countdownEl.text(`${minutes}:${seconds.toString().padStart(2, '0')}`);
        timeLeft--;

        if (timeLeft < 0) {
            clearInterval(countdownInterval);
            clearInterval(checkInterval);
            alert('⛔ Hết thời gian thanh toán. Giao dịch thất bại.');
        }
    }, 1000);

    let checkInterval;
    setTimeout(() => {
        checkInterval = setInterval(() => {
            $.getJSON('/Payment/CheckGoogleSheetTransaction', function (res) {
                if (res && res.data && res.data.length > 0) {
                    const rows = res.data;
                    for (const row of rows) {
                        const transAmount = parseInt(row.values_0_2);
                        /*console.log(typeof row.values_0_9, row.values_0_9);*/
                        const content = String(row.values_0_9 || "");

                        if (transAmount === parseInt(amount) && content.includes(description)) {
                            clearInterval(countdownInterval);
                            clearInterval(checkInterval);

                            showToast("✅ Thanh toán thành công!", "success");

                            const receiverEmail = $('input[name="ReceiverEmail"]').val().trim();
                            const desc = $('textarea[name="Description"]').val();
                            const method = $('input[name="PaymentMethod"]:checked').next().text();

                            const url = `/Transaction/CreateSecretkey?amount=${amountt}&accountId=${accountId}&paymentMethod=${encodeURIComponent(method)}&description=${encodeURIComponent(desc)}&feePercent=${feePercent}&receiverEmail=${receiverEmail}`;

                            window.location.href = url;
                            return;
                        }
                    }
                }
            });
        }, 3000);
    }, 15000);

    $('#qrPopup').on('hidden.bs.modal', function () {
        clearInterval(countdownInterval);
        clearInterval(checkInterval);
    });
});
