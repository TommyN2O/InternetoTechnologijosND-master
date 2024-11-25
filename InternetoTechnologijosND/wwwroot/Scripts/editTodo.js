document.addEventListener('DOMContentLoaded', function () {
    const editTodoForm = document.getElementById('edit-todo-form');
    editTodoForm.addEventListener('submit', function (event) {
        event.preventDefault();

        const urlParams = new URLSearchParams(window.location.search);
        const todoId = urlParams.get('id');
        const token = localStorage.getItem('token');
        const editedTodoName = document.getElementById('edit-todo-name').value.trim();
        const bottomText = document.getElementById('bottom-text');
        updateTodoById(todoId, editedTodoName, token, bottomText);
    });

    const returnButton = document.getElementById('return-button');
    returnButton.addEventListener('click', function () {
        window.location.href = '../Pages/todos.html';
    });
});

function updateTodoById(todoId, editedTodoName, token, bottomText) {
    fetch('/todos/' + todoId, {
        method: 'PUT',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            name: editedTodoName
        })
    })
        .then(response => {
            if (response.ok) {
                bottomText.innerText = 'Todo updated';

                return response.json();
            } else {
                throw new Error('update failed');
            }
        })
        .catch(error => {
            bottomText.innerText = 'Update failed: ' + error.message;
        });
}