document.addEventListener('DOMContentLoaded', function () {
    // 轮询等待 SwaggerUIBundle 和容器就绪
    let checkExist = setInterval(function () {
        if (window.SwaggerUIBundle && document.querySelector('#swagger-ui')) {
            clearInterval(checkExist);
            // 延迟一点，确保其他脚本都初始化完毕
            setTimeout(initSwaggerUI, 500);
        }
    }, 100);

    function initSwaggerUI() {
        // 从 localStorage 里读出上次保存的 token（不含 "Bearer " 前缀）
        const savedToken = localStorage.getItem('swagger-Authorize') || '';
        debugger;
        // 初始化 Swagger UI，并注入 requestInterceptor
        window.ui = SwaggerUIBundle({
            url: '/swagger/v1/swagger.json',               // ← 换成你的 OpenAPI 描述文件地址
            dom_id: '#swagger-ui',
            presets: [
                SwaggerUIBundle.presets.apis,
                SwaggerUIStandalonePreset
            ],
            layout: "StandaloneLayout",
            requestInterceptor: function (req) {
                if (savedToken) {
                    // 每次发请求都自动带上 Authorization
                    req.headers['Authorization'] = 'Bearer ' + savedToken;
                }
                return req;
            }
        });

        // 如果你还想保留“手动弹出对话框并保存 token”功能，这里监听 modal 上的 Authorize 按钮
        document.querySelector('#swagger-ui').addEventListener('click', function (e) {
            // modal 对话框里的 “Authorize” 提交按钮
            if (e.target.closest('.modal-ux .auth-btn-wrapper > button.authorize')) {
                const input = document.querySelector('#api_key_value');  // 你的输入框 ID
                if (input && input.value.trim()) {
                    // 去掉可能的“Bearer ”前缀，只保存纯 token
                    const tok = input.value.replace(/^Bearer\s+/i, '').trim();
                    localStorage.setItem('swagger-Authorize', tok);
                }
            }
        });
    }
});