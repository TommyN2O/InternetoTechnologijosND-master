document.addEventListener('DOMContentLoaded', function () {
    const loginForm  = document.getElementById('login-form');
    loginForm.addEventListener('submit', function (event) {
        event.preventDefault();

        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;
        const bottomText = document.getElementById('bottom-text');
        createUser(username, password, bottomText);
    });
});

function createUser(username, password, bottomText) {
    fetch('/users/createToken', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            username: username,
            password: password
        })
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error('wrong credentials');
            }
        })
        .then(userData => {
            localStorage.setItem('token', userData.token);
            //localStorage.setItem('username', userData.username);
            window.location.href = 'Pages/todos.html';
        })
        .catch(error => {
            bottomText.innerText = 'Login failed: ' + error.message;
        });
}