USE [Master10Dashboard];

create table Users(
  Id int IDENTITY(1,1) not null,
    constraint pk_Users primary key (Id),
  LoginName nvarchar(100) not null,
  Password nvarchar(250) not null,
  FirstName nvarchar(100) not null,
  LastName nvarchar(100) not null,
  Role int not null);
GO
create unique index UsersLoginIndex on Users(LoginName);
GO
create table Targets(
  Id int IDENTITY(1,1) not null,
    constraint pk_Targets primary key (Id),
  Year int not null,
  Quarter int not null,
  Summ numeric(15,2) not null,
  Month1Weight double precision not null,
  Month2Weight double precision not null,
  Month3Weight double precision not null,
  Color nvarchar(255) not null
);
GO
create unique index TargetQuarterIndex on Targets(Year, Quarter);
GO
create table TargetCharts(
  Id int IDENTITY(1,1) not null,
    constraint pk_TargetCharts primary key (Id),
  TargetId int not null,
    constraint [fk_TargetCharts_TargetId] foreign key([TargetId]) references Targets([Id]) on delete cascade,
  Name nvarchar(30) not null,
  Sort int not null,
  Coeff double precision default 1 not null,
  Color nvarchar(255) not null);
GO
create unique index TargetChartNameIndex on TargetCharts(TargetId,Name);
create index TargetChartSortIndex on TargetCharts(TargetId,Sort);
GO
create table WorkCalendar(
  Id int IDENTITY(1,1) not null,
    constraint pk_WorkCalendar primary key (Id),
  Day datetime2 not null,
  IsHoliday bit not null);
GO
create unique index WorkCalendarDayIndex on WorkCalendar(Day);
GO
