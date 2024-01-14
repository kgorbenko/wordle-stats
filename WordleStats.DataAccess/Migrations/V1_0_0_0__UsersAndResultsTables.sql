create table Users (
    Id int not null identity primary key,
    Name nvarchar(255) not null unique
)

create table Results (
    Id int not null identity primary key,
    UserId int not null,

    constraint FK_UserId_Id foreign key (UserId)
        references Users (Id)
        on delete no action
)