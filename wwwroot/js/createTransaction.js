$(document).ready(function () {
    const $formSection = $('#formSection');
    const $confirmSection = $('#confirmationSection');

    $('#previewBtn').on('click', function () {
        const amount = $('input[name="Amount"]').val();
        const desc = $('textarea[name="Description"]').val();
        const expire = $('select[name="ExpirationDays"] option:selected').text();
        const method = $('input[name="PaymentMethod"]:checked').next().text();

        if (!amount || !desc || !$('input[name="PaymentMethod"]:checked').val() || $('select[name="ExpirationDays"]').val() == '') {
            showToast("Vui lòng điền đầy đủ thông tin.", "error");
            return;
        }

        // Cập nhật nội dung
        $('#confirmAmount').text(`${parseInt(amount).toLocaleString()} VNĐ`);
        $('#confirmDescription').text(desc);
        $('#confirmExpiration').text(expire);
        $('#confirmPayment').text(method);

        // Ẩn form, hiện confirm
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

    function showToast(message, type = "success") {
        const toastId = "toast-" + Date.now();

        let bgColor = "bg-success";
        let iconHtml = '<i class="fas fa-check-circle me-2 text-white"></i>'; // Icon mặc định

        if (type === "error" || type === "danger") {
            bgColor = "bg-danger";
            iconHtml = '<i class="fas fa-exclamation-triangle me-2 text-warning"></i>';
        }

        const toastHtml = `
                    <div id="${toastId}" class="toast align-items-center text-white ${bgColor} border-0 show mb-2 shadow" role="alert" aria-live="assertive" aria-atomic="true"
                         style="min-width: 320px;">
                        <div class="d-flex">
                            <div class="toast-body d-flex align-items-center">
                                ${iconHtml}
                                <span>${message}</span>
                            </div>
                            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                        </div>
                    </div>`;

        $("#toast-container").append(toastHtml);

        // Tự động ẩn sau 4 giây
        setTimeout(() => {
            $(`#${toastId}`).fadeOut(300, function () {
                $(this).remove();
            });
        }, 3000);
    }
});

$('#confirmBtn').click(function () {
    const amount = $('input[name="Amount"]').val();
    const accountId = @AccountId;
    const now = new Date();
    const timestamp = `${now.getHours()}${now.getMinutes()}${now.getSeconds()}`;
    const description = `${accountId}${timestamp}`;
    const ImageUrl = `https://img.vietqr.io/image/MB-0868090203-compact2.png?amount=${amount}&addInfo=${description}&accountName=HA VAN SANG`;

    // Hiển thị QR và countdown
    $('#qrImage').attr('src', qrImageUrl).removeClass('d-none');
    $('#qrInstruction').text(`Hoàn tất chuyển khoản trong 10 phút`);
    $('#qrPopup').removeClass('d-none');

    // Start 10-minute countdown
    let timeLeft = 600; // 10 minutes in seconds
    const countdownEl = $('#countdownTimer');
    const countdownInterval = setInterval(() => {
        const minutes = Math.floor(timeLeft / 60);
        const seconds = timeLeft % 60;
        countdownEl.text(`${minutes}:${seconds.toString().padStart(2, '0')}`);
        timeLeft--;

        if (timeLeft < 0) {
            clearInterval(countdownInterval);
            clearInterval(checkInterval);
            alert('Thanh toán thất bại, vui lòng tạo giao dịch khác');
        }
    }, 1000);

    // Delay 30s before starting check
    let checkInterval;
    setTimeout(() => {
        checkInterval = setInterval(() => {
            $.getJSON('/Payment/CheckGoogleSheetTransaction', function (res) {
                if (res && res.data && res.data.length > 1) {
                    const rows = res.data.slice(1);
                    for (const row of rows) {
                        const transAmount = parseInt(row.values_0_2);
                        const content = row.values_0_9;

                        if (transAmount === parseInt(amount) && content.includes(description)) {
                            clearInterval(countdownInterval);
                            clearInterval(checkInterval);
                            alert("✅ Thanh toán thành công!");
                            return;
                        }
                    }
                }
            });
        }, 5000);
    }, 30000);
});
