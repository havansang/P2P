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




$(document).ready(function () {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Auto-hide alerts after 5 seconds
    $('.alert-dismissible').each(function () {
        var alert = $(this);
        setTimeout(function () {
            alert.alert('close');
        }, 5000);
    });

    // Format transaction key input
    $('input[name="TransactionKey"]').on('input', function () {
        var value = $(this).val().toUpperCase().replace(/[^A-Z0-9]/g, '');
        if (value.length > 3) {
            value = value.substring(0, 3) + '-' + value.substring(3);
        }
        if (value.length > 7) {
            value = value.substring(0, 7) + '-' + value.substring(7);
        }
        if (value.length > 11) {
            value = value.substring(0, 11);
        }
        $(this).val(value);
    });

    // Copy to clipboard functionality
    window.copyToClipboard = function (text) {
        if (navigator.clipboard) {
            navigator.clipboard.writeText(text).then(function () {
                showToast('Đã sao chép vào clipboard!', 'success');
            }).catch(function (err) {
                console.error('Failed to copy: ', err);
                fallbackCopyTextToClipboard(text);
            });
        } else {
            fallbackCopyTextToClipboard(text);
        }
    };

    // Fallback copy method
    function fallbackCopyTextToClipboard(text) {
        var textArea = document.createElement("textarea");
        textArea.value = text;

        // Avoid scrolling to bottom
        textArea.style.top = "0";
        textArea.style.left = "0";
        textArea.style.position = "fixed";

        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();

        try {
            var successful = document.execCommand('copy');
            if (successful) {
                showToast('Đã sao chép vào clipboard!', 'success');
            } else {
                showToast('Không thể sao chép. Vui lòng thử lại.', 'error');
            }
        } catch (err) {
            console.error('Fallback: Could not copy text: ', err);
            showToast('Không thể sao chép. Vui lòng thử lại.', 'error');
        }

        document.body.removeChild(textArea);
    }

    // Form validation enhancement
    //$('form').on('submit', function () {
    //    var $form = $(this);
    //    var $submitBtn = $form.find('button[type="submit"]');

    //    if ($form.valid && $form.valid()) {
    //        $submitBtn.prop('disabled', true);
    //        var originalText = $submitBtn.html();
    //        $submitBtn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Đang xử lý...');

    //        // Re-enable button after 10 seconds as fallback
    //        setTimeout(function () {
    //            $submitBtn.prop('disabled', false);
    //            $submitBtn.html(originalText);
    //        }, 10000);
    //    }
    //});

    // Auto-refresh transaction status (if on transactions page)
    if (window.location.pathname.includes('/transactions') || window.location.pathname.includes('/Transaction/Details')) {
        setInterval(function () {
            // Check if there are any pending transactions
            if ($('.badge:contains("Đang chờ")').length > 0) {
                // Refresh the page every 30 seconds if there are pending transactions
                location.reload();
            }
        }, 30000);
    }

    // Amount input formatting
    $('input[name="Amount"]').on('input', function () {
        var value = $(this).val();
        // Remove non-numeric characters except decimal point
        value = value.replace(/[^0-9.]/g, '');

        // Ensure only one decimal point
        var parts = value.split('.');
        if (parts.length > 2) {
            value = parts[0] + '.' + parts.slice(1).join('');
        }

        // Limit decimal places to 2
        if (parts[1] && parts[1].length > 2) {
            value = parts[0] + '.' + parts[1].substring(0, 2);
        }

        $(this).val(value);
    });

    // Navigation active state
    var currentPath = window.location.pathname;
    $('.navbar-nav .nav-link').each(function () {
        var linkPath = $(this).attr('href');
        if (currentPath === linkPath || (linkPath !== '/' && currentPath.startsWith(linkPath))) {
            $(this).addClass('active');
        }
    });
});

// Global functions
window.refreshPage = function () {
    location.reload();
};

window.goBack = function () {
    if (document.referrer && document.referrer.indexOf(window.location.hostname) !== -1) {
        history.back();
    } else {
        window.location.href = '/';
    }
};