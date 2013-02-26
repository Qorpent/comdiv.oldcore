set dateformat ymd
go

if(object_id('mas.testprovider')is null)begin
	create table mas.testprovider (
		id		int identity(1,1) primary key,
		code	nvarchar(255) not null unique default newid(),
		name	nvarchar(255) not null default 'noname',
		version datetime not null default  getdate(),
		class	nvarchar(255) not null default '',
		config		nvarchar(max) not null default '',
	)
end
go

--drop table mas.testresult
if(object_id('mas.testresult')is null)begin
	create table mas.testresult (
		id		int identity(1,1) primary key,
		code	nvarchar(255) not null unique default newid(),
		name	nvarchar(255) not null default 'noname',
		version datetime not null default  getdate(),
		type nvarchar(255) not null default 'default',
		level int not null default 1,
		message nvarchar(max) not null default '',
		xmldata xml not null default '<empty/>',
		canbeignored bit not null default 1,
		ignored bit not null default 0,
		fixed bit not null default 0,
		active bit not null default 1,
		test nvarchar(255) not null default 'default',
		system nvarchar(255) not null default 'default',
		application nvarchar(255) not null default 'default',
		providerid int  null foreign key references mas.testprovider (id)
      
	)
end
go
if OBJECT_ID('mas.testresultupdate') is not null drop view mas.testresultupdate
go
create view mas.testresultupdate as select top 1 * from mas.testresult
go
create trigger testresultupdate_insert_update on mas.testresultupdate instead of insert, update as begin
	select * into #u from inserted  where exists(select top 1 id from mas.testresult t where t.code = inserted.code)
	select * into #i from inserted  where not exists(select top 1 id from mas.testresult t where t.code = inserted.code)
	insert mas.testresult(
		code, name, version, type, level, message, xmldata, canbeignored, ignored, fixed, active, test, system, application, providerid,
		var1,var2,var3,var4,link,autofix)
	select 
		isnull(code,newid()), 
		isnull(name,code), 
		isnull(version,getdate()),
		ISNULL(type,'default'),
		ISNULL(level,3),
		ISNULL(message,''),
		ISNULL(xmldata,'<empty/>'),
		ISNULL( canbeignored, 1),
		ISNULL( ignored,0),
		ISNULL( fixed,0),
		ISNULL( active,1),
		ISNULL( test, ''),
		ISNULL( system, @@SERVERNAME),
		ISNULL( application, db_name()), providerid,
		var1,var2,var3,var4,link,autofix
		 from #i
	update mas.testresult 
		set 
		name = ISNULL(#u.name,mas.testresult.name),
		version = GETDATE(),
		type = ISNULL(#u.type,mas.testresult.type),
		level = ISNULL(#u.level,mas.testresult.level),
		message = ISNULL(#u.message,mas.testresult.message),
		xmldata = ISNULL(#u.xmldata,mas.testresult.xmldata),
		canbeignored = ISNULL(#u.canbeignored, mas.testresult.canbeignored),
		ignored = ISNULL(#u.ignored, mas.testresult.ignored),
		fixed = ISNULL(#u.fixed, mas.testresult.fixed),
		active = ISNULL(#u.active, mas.testresult.active),
		test = ISNULL(#u.test, mas.testresult.test),
		system = ISNULL(#u.system, mas.testresult.system),
		application = ISNULL(#u.application, mas.testresult.application),
		providerid = ISNULL(#u.providerid, mas.testresult.providerid),
		var1 = ISNULL(#u.var1, mas.testresult.var1),
		var2 = ISNULL(#u.var2, mas.testresult.var2),
		var3 = ISNULL(#u.var3, mas.testresult.var3),
		var4 = ISNULL(#u.var4, mas.testresult.var4),
		link = ISNULL(#u.link, mas.testresult.link),
		autofix = ISNULL(#u.autofix, mas.testresult.autofix)
	from #u where #u.code = mas.testresult.code
end

go
if(object_id('mas.process')is null)begin
	create table mas.process (
		id		int identity(1,1) primary key,
		code	nvarchar(255) not null unique default newid(),
		name	nvarchar(255) not null default 'noname',
		version datetime not null default  getdate(),
		host	nvarchar(30) not null default  'nohost',
		args	nvarchar(255) not null default '',
		pid		int not null default 0,
		state	nvarchar(30) not null default '',
		usr		nvarchar(30)not null default 'sys',
		isactive bit not null default 1,
		result	int  not null default -10,
		resultdescription nvarchar(255) not null default '',
		starttime datetime not null default getdate(),
		endtime	datetime not null default '3000-01-01'
	)
end
go



if(object_id('mas.processmessage')is null)begin
	create table mas.processmessage (
		id			int identity(1,1) primary key,
		version		datetime not null default getdate(),
		processid	int null foreign key references mas.process(id) on delete cascade,
		sender		nvarchar(255) not null default 'sys',
		priority	int not null default 0,
		type		nvarchar(30) not null default 'default',
		message		nvarchar(max) not null default '',
		answer		nvarchar(max) not null default '',
		accepted	bit not null default 0,
		processed	bit not null default 0,
		sendtime	datetime not null default getdate(),
		answertime	datetime not null default '3000-01-01'
	)
end

go

if(object_id('mas.processlog')is null)begin
	create table mas.processlog (
		id			int identity(1,1) primary key,
		version		datetime not null default getdate(),
		processid	int null foreign key references mas.process(id) on delete cascade,
		sender		nvarchar(255) not null default 'sys',
		type		int not null default 0,
		message		nvarchar(max) not null default '',
		[event]		nvarchar(255) not null default ''
	)
end
go

if(object_id('mas.server')is null)begin
	create table mas.server (
		id			int identity(1,1) primary key,
		code		nvarchar (255) not null default newid(),
		version		datetime not null default getdate(),
		name		nvarchar(255) not null default '',
		lneturl		nvarchar(255) not null default '',
		ineturl		nvarchar(255) not null default '',
		comment		nvarchar(255) not null default '',
		config		nvarchar(max) not null default ''
	)
end
go

if(object_id('mas.apptype')is null)begin
	create table mas.apptype (
		id			int identity(1,1) primary key,
		code		nvarchar(255) not null default newid(),
		version		datetime not null default getdate(),
		name		nvarchar(255) not null default '',
		comment		nvarchar(255) not null default '',
		commands	nvarchar(max) not null default '',
		config		nvarchar(max) not null default '',
		parent		int null foreign key references mas.apptype (id)
	)
end
go
if(object_id('mas.app')is null)begin
	create table mas.app (
		id			int identity(1,1) primary key,
		code		nvarchar(255) not null default newid(),
		version		datetime not null default getdate(),
		name		nvarchar(255) not null default '',
		comment		nvarchar(255) not null default '',
		type		int	not null foreign key references mas.apptype (id),
		server		int not null foreign key references mas.server (id),
		config		nvarchar(max) not null default ''
	)
end
go
/*
drop table mas.app
drop table mas.apptype
drop table mas.server
*/

select * from mas.apptype
