namespace FlyzenApi.API.StaticPages
{
    public static class ResetPasswordPage
    {
        public const string Html = @"<!doctype html>
<html lang=""en"">
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
<title>Reset your SkyBook password</title>
<style>
  :root { color-scheme: light dark; }
  * { box-sizing: border-box; }
  body {
    margin: 0;
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: #F1F5F9;
    font-family: -apple-system, ""Segoe UI"", Helvetica, Arial, sans-serif;
    padding: 24px;
  }
  @media (prefers-color-scheme: dark) {
    body { background: #0F172A; }
  }
  .card {
    width: 100%;
    max-width: 400px;
    background: #fff;
    border-radius: 24px;
    overflow: hidden;
    box-shadow: 0 10px 40px rgba(0,0,0,0.12);
  }
  @media (prefers-color-scheme: dark) {
    .card { background: #1E1E2A; }
  }
  .header {
    background: linear-gradient(135deg,#1E3A8A,#3B82F6);
    padding: 28px 32px;
    color: #fff;
  }
  .header h1 { margin: 0; font-size: 20px; font-weight: 800; letter-spacing: 0.5px; }
  .body { padding: 28px 32px; }
  label { display:block; font-size: 13px; font-weight:600; color:#475569; margin-bottom:6px; }
  input[type=password] {
    width: 100%;
    height: 48px;
    border-radius: 12px;
    border: 1px solid #E2E8F0;
    padding: 0 14px;
    font-size: 15px;
    margin-bottom: 16px;
    background: #F8FAFC;
    color: #0F172A;
  }
  @media (prefers-color-scheme: dark) {
    input[type=password] { background:#262633; border-color:#33333F; color:#F1F5F9; }
    label { color: #A6A6B5; }
  }
  button {
    width: 100%;
    height: 48px;
    border: none;
    border-radius: 12px;
    background: #FF6B00;
    color: #fff;
    font-size: 16px;
    font-weight: 700;
    cursor: pointer;
  }
  button:disabled { opacity: 0.6; cursor: default; }
  .message { font-size: 14px; margin-top: 16px; text-align: center; }
  .message.error { color: #DC2626; }
  .message.success { color: #16A34A; }
  .app-link { display:block; text-align:center; margin-top: 18px; font-size: 13px; color:#64748B; text-decoration:none; }
  .hidden { display:none; }
</style>
</head>
<body>
  <div class=""card"">
    <div class=""header""><h1>Reset your SkyBook password</h1></div>
    <div class=""body"">
      <form id=""form"">
        <label for=""password"">New password</label>
        <input id=""password"" type=""password"" minlength=""6"" required autocomplete=""new-password"" />
        <label for=""confirm"">Confirm password</label>
        <input id=""confirm"" type=""password"" minlength=""6"" required autocomplete=""new-password"" />
        <button id=""submit"" type=""submit"">Reset password</button>
      </form>
      <div id=""message"" class=""message hidden""></div>
      <a id=""appLink"" class=""app-link hidden"" href=""#"">Open in the SkyBook app instead</a>
    </div>
  </div>
  <script>
    var params = new URLSearchParams(window.location.search);
    var token = params.get('token');
    var form = document.getElementById('form');
    var submitBtn = document.getElementById('submit');
    var messageEl = document.getElementById('message');
    var appLink = document.getElementById('appLink');

    if (token) {
      appLink.href = 'skybook://reset-password?token=' + encodeURIComponent(token);
      appLink.classList.remove('hidden');
    }

    function showMessage(text, type) {
      messageEl.textContent = text;
      messageEl.className = 'message' + (type ? ' ' + type : '');
    }

    if (!token) {
      form.classList.add('hidden');
      showMessage('This reset link is missing its token. Please request a new one from the app.', 'error');
    }

    form.addEventListener('submit', function (e) {
      e.preventDefault();
      var password = document.getElementById('password').value;
      var confirm = document.getElementById('confirm').value;

      if (password.length < 6) {
        showMessage('Password must be at least 6 characters.', 'error');
        return;
      }
      if (password !== confirm) {
        showMessage('Passwords do not match.', 'error');
        return;
      }

      submitBtn.disabled = true;
      submitBtn.textContent = 'Resetting...';
      showMessage('', '');

      fetch('/api/auth/reset-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token: token, newPassword: password }),
      })
        .then(function (res) {
          return res.text().then(function (text) {
            var data = text ? JSON.parse(text) : null;
            if (!res.ok) throw new Error((data && data.message) || 'Something went wrong.');
            return data;
          });
        })
        .then(function (data) {
          form.classList.add('hidden');
          showMessage((data && data.message) || 'Your password has been reset. You can now log in with your new password in the app.', 'success');
        })
        .catch(function (err) {
          showMessage(err.message || 'Could not reset your password. The link may have expired.', 'error');
          submitBtn.disabled = false;
          submitBtn.textContent = 'Reset password';
        });
    });
  </script>
</body>
</html>
";
    }
}
