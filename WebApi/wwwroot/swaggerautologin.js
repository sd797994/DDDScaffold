document.addEventListener('DOMContentLoaded', function () {
    let checkExist = setInterval(function () {
        var dom = document.querySelector('.swagger-ui')
        if (dom) {
            clearInterval(checkExist);
            // 延时半秒执行，为了让自身的代码先执行完成
            setTimeout(setTestToken, 500);
            // 监听点击Authorize按钮，自动填入token
            document.querySelector('.swagger-ui').addEventListener('click', function (e) {
                if (e.target.closest('.schemes .auth-wrapper > .authorize')) {
                    setTimeout(function () {
                        var input = document.querySelector('.swagger-ui .dialog-ux input');
                        var value = input.value;
                        if (value === "") {
                            var token = localStorage.getItem("swagger-Authorize");
                            if (token && token !== "") {
                                // 使用原生 DOM 元素
                                input.value = token;
                                // 触发 input 事件
                                var event = new Event('input', { bubbles: true });
                                event.simulated = true; // 标记为模拟事件
                                input.dispatchEvent(event);

                                // 提交token
                                setTimeout(function () {
                                    if (isAutoToken) {
                                        isAutoToken = false;
                                        var submitForm = document.querySelector('.swagger-ui .modal-ux .auth-container form');

                                        var event2 = new Event('submit', { bubbles: true });
                                        event2.simulated = true;
                                        if (submitForm)
                                            submitForm.dispatchEvent(event2);

                                        // 关闭 Authorized 框
                                        var close = document.querySelector('.swagger-ui .modal-ux .modal-ux-header .close-modal');
                                        var event3 = new Event('click', { bubbles: true });
                                        event3.simulated = true;
                                        if (close)
                                            close.dispatchEvent(event3);
                                    }
                                }, 100);
                            }
                        }
                    }, 100);
                }
            });

            // 保存token
            document.querySelector('.swagger-ui').addEventListener('click', function (e) {
                if (e.target.closest('.modal-ux .auth-btn-wrapper > button.authorize')) {
                    var input = document.querySelector('.swagger-ui .dialog-ux input');
                    localStorage.setItem("swagger-Authorize", input.value);
                }
            });
        }
    }, 100);
});
// 自动和手动区分的变量
var isAutoToken = false;

// 自动点击Authorize弹出输入框
function setTestToken() {
    var token = localStorage.getItem("swagger-Authorize");
    if (token && token !== "") {
        isAutoToken = true; // 是第一次自动显示

        var authorize = document.querySelector('.swagger-ui .schemes .auth-wrapper > button.authorize');
        var event1 = new Event('click', { bubbles: true });
        event1.simulated = true;
        if (authorize)
            authorize.dispatchEvent(event1);
    }
}
