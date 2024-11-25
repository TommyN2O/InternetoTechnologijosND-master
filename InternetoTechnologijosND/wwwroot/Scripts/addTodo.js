document.addEventListener('DOMContentLoaded', function () {
    const newTodoForm  = document.getElementById('new-todo-form');
    newTodoForm.addEventListener('submit', function (event) {
        event.preventDefault();

        const token = localStorage.getItem('token');
        const newTodoName = document.getElementById('new-todo-name').value.trim();
        const bottomText = document.getElementById('bottom-text');
        createTodo(token, newTodoName, bottomText);
    });

    const returnButton = document.getElementById('return-button');
    returnButton.addEventListener('click', function () {
        window.location.href = '../Pages/todos.html';
    });
});

function createTodo(token, newTodoName, bottomText) {
    fetch('/todos', {
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            name: newTodoName
        })
    })
        .then(response => {
            if (response.ok) {
                bottomText.innerText = 'Todo created';

                return response.json();
            } else {
                throw new Error('create failed');
            }
        })
        .catch(error => {
            bottomText.innerText = 'Create failed: ' + error.message;
        });
}
