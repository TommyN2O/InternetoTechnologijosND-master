document.addEventListener('DOMContentLoaded', function () {
    const addTodoButton       = document.getElementById('add-todo-button');
    addTodoButton.addEventListener('click', function () {
        window.location.href = '../Pages/addTodo.html';
    });

    const returnToLoginButton = document.getElementById('return-to-login-button');
    returnToLoginButton.addEventListener('click', function () {
        window.location.href = '../';
    });

    const token = localStorage.getItem('token');
    addTodoToList(token);
});

function addTodoToList(token) {
    fetch('/todos', {
        headers: {
            'Authorization': 'Bearer ' + token
        }
    })
        .then(response => response.json())
        .then(todos => {
            const todosList = document.getElementById('todos-list');

            todos.forEach(todo => {
                const todoItem = document.createElement('li');
                todoItem.textContent = todo.name;
                todoItem.classList.add('todos-design');

                if (todo.isComplete) {
                    todoItem.classList.add('completed');

                    const deleteButton = document.createElement('button');
                    deleteButton.textContent = '✘';
                    deleteButton.classList.add('delete-button');
                    todoItem.appendChild(deleteButton);
                    deleteButton.addEventListener('click', function () {
                        deleteTodoById(todo.id, todoItem, todosList);
                    });

                } else {
                    const updateButton = document.createElement('button');
                    updateButton.textContent = '✔';
                    updateButton.classList.add('update-button');
                    todoItem.appendChild(updateButton);
                    updateButton.addEventListener('click', function () {
                        updateTodoToCompleteById(todo.id, todoItem, todosList);
                    })

                    const editButton = document.createElement('button');
                    editButton.textContent = '✎';
                    editButton.classList.add('edit-button');
                    todoItem.appendChild(editButton);
                    editButton.addEventListener('click', function () {
                        window.location.href = '../Pages/editTodo.html?id=' + todo.id;
                    });
                }

                todosList.appendChild(todoItem);
            });
        })
        .catch(error => {
            //alert(error);
        });
}

function deleteTodoById(id, todoItem, todosList) {
    const token = localStorage.getItem('token');

    fetch('/todos/' + id, {
        method: 'DELETE',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    })
        .then(response => {
            if (response.ok) {
                todosList.removeChild(todoItem);
            } else {
                throw new Error('Failed to delete todo');
            }
        })
        .catch(error => {
            alert(error);
        });
}

function updateTodoToCompleteById(id, todoItem, todosList) {
    const token = localStorage.getItem('token');

    fetch('/todos/complete/' + id, {
        method: 'PUT',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({})
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error('Failed to update todo');
            }
        })
        .then(completedTodo => {
            todoItem.textContent = completedTodo.name;
            todoItem.classList.add('completed');

            const deleteButton = document.createElement('button');
            deleteButton.textContent = '✘';
            deleteButton.classList.add('delete-button');
            todoItem.appendChild(deleteButton);
            deleteButton.addEventListener('click', function () {
                deleteTodoById(completedTodo.id, todoItem, todosList);
            });
        })
        .catch(error => {
            alert(error);
        });
}