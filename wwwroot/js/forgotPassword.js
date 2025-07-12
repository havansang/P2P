$(document).ready(function () {
    const $form = $('#registrationForm');
    const $form1 = $('#changePasswordForm');
    const $submitBtn = $form.find('button[type="submit"]');
    const $submitBtn1 = $form1.find('button[type="submit"]');
    const originalText = $submitBtn.html();
    const originalText1 = $submitBtn1.html();
    const $sendCodeBtn = $('#sendCodeBtn');

    // Gửi mã xác nhận
    $("#sendCodeBtn").click(function () {
        const email = $("input[name='email']").val();

        if (!email) {
            showToast("Vui lòng nhập email trước khi gửi mã.", "error");
            return;
        }
        $(this).prop("disabled", true).addClass("disabled");
        $.get("/Account/SendVerificationCode", { email }, function (res) {
            if (res.success) {
                showToast(res.message, "success");
                startCountdown();
            } else {
                showToast(res.message, "error");
            }
        }).fail(function () {
            showToast("Không thể gửi mã xác nhận. Vui lòng thử lại.", "error");
        });
    });

    // Countdown gửi lại mã
    function startCountdown() {
        let countdown = 30;
        const $btn = $("#sendCodeBtn");

        $btn.prop("disabled", true).text(`Gửi lại (${countdown}s)`);
        const timer = setInterval(() => {
            countdown--;
            if (countdown <= 0) {
                clearInterval(timer);
                $btn.prop("disabled", false).text("Gửi mã");
                $sendCodeBtn.prop('disabled', false).removeClass('disabled');
            } else {
                $btn.text(`Gửi lại (${countdown}s)`);

            }
        }, 1000);
    }

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

    $form.on('submit', function (e) {
        e.preventDefault();

        $submitBtn.prop('disabled', true);
        $submitBtn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Đang xử lý...');

        $.post("/Account/ForgotPassword", $form.serialize())
            .done(function (res) {
                if (res.success) {
                    showChangePasswordView();
                } else {
                    showToast(res.message, "error");
                }
            })
            .fail(function () {
                showToast("Có lỗi xảy ra khi xác nhận mã. Vui lòng thử lại.", "error");
            })
            .always(function () {
                $submitBtn.prop('disabled', false);
                $submitBtn.html(originalText);
            });
    });
    function showChangePasswordView() {
        $("#step1").addClass("d-none");
        $("#step2").removeClass("d-none");
    }

    // Submit form2
    $form1.on('submit', function (e) {
        e.preventDefault();

        $submitBtn1.prop('disabled', true);
        $submitBtn1.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Đang xử lý...');

        const password = $('#password').val();
        const confirmPassword = $('#confirmPassword').val();

        console.log('Mật khẩu:', password);
        console.log('Xác nhận mật khẩu:', confirmPassword);

        $.post("/Account/ConfirmChangePassword", $form1.serialize())
            .done(function (res) {
                if (res.success) {
                    showToast("Đổi mật khẩu thành công! Đang chuyển hướng...", "success");
                    setTimeout(() => {
                        window.location.href = res.redirectUrl || "/";
                    }, 2000);
                } else {
                    showToast(res.message, "error");
                }
            })
            .fail(function () {
                showToast("Có lỗi xảy ra. Vui lòng thử lại.", "error");
            })
            .always(function () {
                $submitBtn1.prop('disabled', false);
                $submitBtn1.html(originalText1);
            });
    });
});