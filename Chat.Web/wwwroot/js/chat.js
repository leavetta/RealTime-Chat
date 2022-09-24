$(document).ready(function () {
    var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

    connection.start().then(function () {
        console.log('SignalR Started...')
        viewModel.roomList();
        viewModel.userList();
        viewModel.userConnections();
    }).catch(function (err) {
        return console.error(err);
    });

    connection.on("newMessage", function (messageView) {
        var isMine = messageView.from === viewModel.myName();
        var message = new ChatMessage(messageView.content, messageView.timestamp, messageView.from, isMine, messageView.avatar);
        viewModel.chatMessages.push(message);
        $(".chat-body").animate({ scrollTop: $(".chat-body")[0].scrollHeight }, 1000);
    });

    connection.on("getProfileInfo", function (displayName, avatar) {
        viewModel.myName(displayName);
        viewModel.myAvatar(avatar);
        viewModel.isLoading(false);
    });

    connection.on("addUser", function (user) {
        viewModel.userAdded(new ChatUser(user.username, user.avatar, user.currentRoom, user.device),
            new ChatUserRooms(user.username, user.avatar, user.currentRoom, user.device));
    });

    connection.on("removeUser", function (user) {
        viewModel.userRemoved(user.username);
    });

    connection.on("addChatRoom", function (room) {
        viewModel.roomAdded(new ChatRoom(room.id, room.name));
        location.reload();
    });

    connection.on("updateChatRoom", function (room) {
        viewModel.roomUpdated(new ChatRoom(room.id, room.name));
    });

    connection.on("removeChatRoom", function (id) {
        viewModel.roomDeleted(id);
    });

    connection.on("onError", function (message) {
        viewModel.serverInfoMessage(message);
        $("#errorAlert").removeClass("d-none").show().delay(5000).fadeOut(500);
    });


    $('ul#users-list2').on('click', 'li', function () {
        var username = $("input[type=hidden].username", $(this)).val();
        var result = confirm("Хотите добавить в чат пользователя " + username + "?");
        if (result) {
            $("#myDropdown1").toggleClass("d-none");
            viewModel.AddUserToChat(username);
        } 
        //viewModel.createRoomPrivate(username);
        console.log(username);
    });

    $('ul#users-listChat').on('click', 'li', function () {
        var username = $("input[type=hidden].username", $(this)).val();
        var result = confirm("Хотите удалить пользователя " + username + " из чата?");
        if (result) {
            $("#showUsersCh").toggleClass("d-none");
            viewModel.DeleteUserFromChat(username);
        }
        //viewModel.createRoomPrivate(username);
        console.log(username);
    });

    $('ul#users-list').on('click', 'li', function () {
        var username = $("input[type=hidden].username", $(this)).val();
        viewModel.createRoomPrivate(username);

    });
    var isOnlineShown = false;
    $("#Dropbtn1").click(function () {
        $("#myDropdown").toggleClass("d-none");
        isOnlineShown = !isOnlineShown;
        if (isOnlineShown) {
            document.getElementById("Dropbtn1").innerHTML = "Пользователи онлайн &#9660;";
        }
        else {
            document.getElementById("Dropbtn1").innerHTML = "Пользователи онлайн &#10148;";
        }
    });

    var isOnlineShown1 = false;
    $("#Dropbtn2").click(function () {
        $("#myDropdown1").toggleClass("d-none");
        isOnlineShown1 = !isOnlineShown1;
        if (isOnlineShown1) {
            document.getElementById("Dropbtn2").innerHTML = "Добавить пользователя &#9660;";
        }
        else {
            document.getElementById("Dropbtn2").innerHTML = "Добавить пользователя &#10148;";
        }
        viewModel.printUsersNotInChat();
    });

    var isOnlineShown2 = false;
    $("#showUsersInChat").click(function () {
        $("#showUsersCh").toggleClass("d-none");
        isOnlineShown2 = !isOnlineShown2;
        if (isOnlineShown2) {
            document.getElementById("showUsersInChat").innerHTML = "Показать участников &#9660;";
        }
        else {
            document.getElementById("showUsersInChat").innerHTML = "Показать участников &#10148;";
        }
        //viewModel.printUsersInChat();
    });

    connection.on("onRoomDeleted", function (message) {
        viewModel.serverInfoMessage(message);
        $("#errorAlert").removeClass("d-none").show().delay(5000).fadeOut(500);

        if (viewModel.chatRooms().length == 0) {
            viewModel.joinedRoom("");
        }
        else {
            // Join to the first room in list
            setTimeout(function () {
                $("ul#room-list li a")[0].click();
            }, 50);
        }
    });



    function AppViewModel() {
        var self = this;

        self.message = ko.observable("");
        self.chatRooms = ko.observableArray([]);
        self.chatUsers = ko.observableArray([]);
        self.chatUsersList = [];
        self.tempUsersConnection = [];
        self.chatUsersOnline = ko.observableArray([]);
        self.usersinChat = [];
        self.usersIsInChat = ko.observableArray([]);
        self.usersNotInChat = ko.observableArray([]);
        self.chatMessages = ko.observableArray([]);
        self.joinedRoom = ko.observable("");
        self.joinedRoomId = ko.observable("");
        self.serverInfoMessage = ko.observable("");
        self.myName = ko.observable("");
        self.checkUsers = ko.observable("");
        self.myAvatar = ko.observable("avatar1.png");
        self.isLoading = ko.observable(true);

        self.onEnter = function (d, e) {
            if (e.keyCode === 13) {
                self.sendNewMessage();
            }
            return true;
        }
        self.filter = ko.observable("");
        self.filteredChatUsers = ko.computed(function () {
            //self.userList();
            if (!self.filter()) {
                //self.checkUsers();
                return self.chatUsers();
            } else {
                //self.checkUsers();
                return ko.utils.arrayFilter(self.chatUsers(), function (user) {
                    var userName = user.userName().toLowerCase();
                    return userName.includes(self.filter().toLowerCase());
                });
            }
        });

        self.filterOnline = ko.observable("");
        self.filteredChatUsersOnline = ko.computed(function () {
            //self.userList();
            if (!self.filter()) {
                //self.checkUsers();
                return self.chatUsersOnline();
            } else {
                //self.checkUsers();
                return ko.utils.arrayFilter(self.chatUsersOnline(), function (user) {
                    var userName = user.userName().toLowerCase();
                    return userName.includes(self.filter().toLowerCase());
                });
            }
        });

        self.returnUsersIsNotInThisChat = ko.computed(function () {
            //self.printUsersInChat();
            return self.usersNotInChat();
        });

        self.returnUsersInThisChat = ko.computed(function () {
            //self.printUsersInChat();
            return self.usersIsInChat();
        });

        self.sendNewMessage = function () {
            var text = self.message();
            if (text.startsWith("/")) {
                var receiver = text.substring(text.indexOf("(") + 1, text.indexOf(")"));
                var message = text.substring(text.indexOf(")") + 1, text.length);
                self.sendPrivate(receiver, message);
            }
            else {
                self.sendToRoom(self.joinedRoom(), self.message());
            }

            self.message("");
        }

        self.sendToRoom = function (roomName, message) {
            if (roomName.length > 0 && message.length > 0) {
                fetch('/api/Messages', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ room: roomName, content: message })
                });
                //connection.invoke("SendAllInGroup", roomName.trim(), message.trim());
                
            }
        }

        self.sendPrivate = function (receiver, message) {
            if (receiver.length > 0 && message.length > 0) {
                connection.invoke("SendPrivate", receiver.trim(), message.trim());
            }
        }

        self.joinRoom = function (room) {
            connection.invoke("Join", room.name()).then(function () {
                
                //$("#myDropdown1").hide();
                
                //$("#myDropdown1").toggleClass(display );
                self.joinedRoom(room.name());
                self.joinedRoomId(room.id());
                self.listUsersInThisChat();
                self.userList();
                self.messageHistory();
            });
        }

        self.roomList = function () {
            fetch('/api/Rooms')
                .then(response => response.json())
                .then(data => {
                    self.chatRooms.removeAll();
                    for (var i = 0; i < data.length; i++) {
                        self.chatRooms.push(new ChatRoom(data[i].id, data[i].name));
                    }

                    /*if (self.chatRooms().length > 0)
                        self.joinRoom(self.chatRooms()[0]);*/
                });
        }

        self.userConnections = function () {
            connection.invoke("GetConnections").then(function (connectionsUsers) {
                self.tempUsersConnection = [];
                for (var i = 0; i < connectionsUsers.length; i++) {
                    self.tempUsersConnection.push(connectionsUsers[i]);
                }
                console.log(connectionsUsers);
            });
            
        }

        self.userList = function () {
            fetch('/api/Users')
                .then(response => response.json())
                .then(data => {
                    self.chatUsers.removeAll();
                    self.chatUsersList = [];
                    for (var i = 0; i < data.length; i++) {
                        self.chatUsersList.push(
                            new ChatUserRooms(data[i].username,
                                data[i].avatar,
                                data[i].currentRoom,
                                data[i].device)
                        )

                        self.chatUsers.push(new ChatUser(data[i].username,
                            data[i].avatar,
                            data[i].currentRoom,
                            data[i].device))
                    }

                    /*if (self.chatRooms().length > 0)
                        self.joinRoom(self.chatRooms()[0]);*/
                });

            connection.invoke("GetUsers", self.joinedRoom()).then(function (result) {
                self.chatUsersOnline.removeAll();
                //self.tempUsers.removeAll();
                console.log(result.length);
                for (var i = 0; i < result.length; i++) {
                    //self.chatUsers.remove(function (item) { return item.userName() == result[i].username });
                    self.chatUsersOnline.push(new ChatUser(result[i].username,
                        result[i].avatar,
                        result[i].currentRoom,
                        result[i].device));
                    console.log(self.chatUsersOnline());
                }

            });
            
        }


        self.listUsersInThisChat = function () {
            var roomId = self.joinedRoomId();
            self.usersinChat = [];
            self.usersIsInChat.removeAll();
            fetch('/api/Users/Room/' + self.joinedRoom())
                .then(response => response.json())
                .then(data => {
                    if (data != null) {
                        //self.usersinChat = [];
                        for (var i = 0; i < data.length; i++) {
                            self.usersinChat.push(new ChatUserRooms(data[i].username,
                                data[i].avatar,
                                data[i].currentRoom,
                                data[i].device))
                        }
                        for (var i = 0; i < data.length; i++) {
                            self.usersIsInChat.push(new ChatUser(data[i].username,
                                data[i].avatar,
                                data[i].currentRoom,
                                data[i].device))
                        }
                    }
                });
            console.log(self.usersIsInChat());
            
        }

        self.printUsersNotInChat = function () {
            
            array3 = self.chatUsersList.filter(n => !self.usersinChat.some(m => m.userName === n.userName))
            console.log(self.chatUsersList);
            console.log(self.usersinChat);
            self.usersNotInChat.removeAll();
            for (var i = 0; i < array3.length; i++) {
                if (array3.length == self.chatUsersList.length) {
                    break;
                }
                else {
                    self.usersNotInChat.push(new ChatUser(array3[i].userName,
                        array3[i].avatar,
                        array3[i].currentRoom,
                        array3[i].device))
                }
                
            }
            console.log(array3[0].userName);
            console.log(self.usersNotInChat());
        }

        
        self.AddUserToChat = function (userName) {
            fetch('/api/Rooms/addUser', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: self.joinedRoomId(), name: userName })
            }).then(function () {
                $("#myDropdown1").toggleClass("d-none");
                location.reload();
                self.listUsersInThisChat();
                
            });
        }

        self.DeleteUserFromChat = function (userName) {
            fetch('/api/Rooms/deleteUser', {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: self.joinedRoomId(), name: userName })
            }).then(function () {
                $("#showUsersCh").toggleClass("d-none");
                location.reload();
                self.listUsersInThisChat();

            });
        }

        self.createRoom = function () {
            var roomName = $("#roomName").val();
            fetch('/api/Rooms', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: roomName })
            }).then(function () {
                location.reload();
            });
        }
        

        self.createRoomPrivate = function (userName) {
            
            fetch('/api/Rooms/private', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: userName })
            }).then(function () {
                location.reload();
            });
        }

        self.editRoom = function () {
            var roomId = self.joinedRoomId();
            var roomName = $("#newRoomName").val();
            fetch('/api/Rooms/' + roomId, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: roomId, name: roomName })
            });
        }

        self.deleteRoom = function () {
            fetch('/api/Rooms/' + self.joinedRoomId(), {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: self.joinedRoomId() })
            });
        }

        self.messageHistory = function () {
            fetch('/api/Messages/Room/' + viewModel.joinedRoom())
                .then(response => response.json())
                .then(data => {
                    self.chatMessages.removeAll();
                    for (var i = 0; i < data.length; i++) {
                        var isMine = data[i].from == self.myName();
                        self.chatMessages.push(new ChatMessage(data[i].content,
                            data[i].timestamp,
                            data[i].from,
                            isMine,
                            data[i].avatar))
                    }

                    $(".chat-body").animate({ scrollTop: $(".chat-body")[0].scrollHeight }, 1000);
                });
        }

        self.roomAdded = function (room) {
            self.chatRooms.push(room);
            
        }

        self.roomUpdated = function (updatedRoom) {
            var room = ko.utils.arrayFirst(self.chatRooms(), function (item) {
                return updatedRoom.id() == item.id();
            });

            room.name(updatedRoom.name());

            if (self.joinedRoomId() == room.id()) {
                self.joinRoom(room);
            }
        }

        self.roomDeleted = function (id) {
            var temp;
            ko.utils.arrayForEach(self.chatRooms(), function (room) {
                if (room.id() == id)
                    temp = room;
            });
            self.chatRooms.remove(temp);
            location.reload();
        }

        self.userAdded = function (user, userRoom) {
            self.chatUsers.push(user);
            self.chatUsersList.push(userRoom);
        }

        self.userRemoved = function (id) {
            var temp;
            ko.utils.arrayForEach(self.chatUsers(), function (user) {
                if (user.userName() == id)
                    temp = user;
            });
            self.chatUsers.remove(temp);
        }

        self.uploadFiles = function () {
            var form = document.getElementById("uploadForm");
            $.ajax({
                type: "POST",
                url: '/api/Upload',
                data: new FormData(form),
                contentType: false,
                processData: false,
                success: function () {
                    $("#UploadedFile").val("");
                },
                error: function (error) {
                    alert('Error: ' + error.responseText);
                }
            });
        }
    }

    function ChatRoom(id, name) {
        var self = this;
        self.id = ko.observable(id);
        self.name = ko.observable(name);
    }

    function ChatUser(userName, avatar, currentRoom, device) {
        var self = this;
        self.userName = ko.observable(userName);
        self.avatar = ko.observable(avatar);
        self.currentRoom = ko.observable(currentRoom);
        self.device = ko.observable(device);
    }

    function ChatUserRooms(userName, avatar, currentRoom, device) {
        var self = this;
        self.userName = userName;
        self.avatar = avatar;
        self.currentRoom = currentRoom;
        self.device = device;
    }

    function ChatMessage(content, timestamp, from, isMine, avatar) {
        var self = this;
        self.content = ko.observable(content);
        self.timestamp = ko.observable(timestamp);
        self.from = ko.observable(from);
        self.isMine = ko.observable(isMine);
        self.avatar = ko.observable(avatar);
    }

    var viewModel = new AppViewModel();
    ko.applyBindings(viewModel);
});
