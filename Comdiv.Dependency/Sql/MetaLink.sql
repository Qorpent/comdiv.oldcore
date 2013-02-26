if SCHEMA_ID('comdiv') is null exec sp_executesql N'create schema comdiv'
go
if OBJECT_ID('comdiv.metalink') is null
CREATE TABLE [comdiv].[metalink](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[code] [nvarchar](255) NOT NULL,
	[srctype] [nvarchar](50) NOT NULL,
	[trgtype] [nvarchar](50) NOT NULL,
	[src] [nvarchar](50) NOT NULL,
	[trg] [nvarchar](50) NOT NULL,
	[type] [nvarchar](20) NOT NULL,
	[subtype] [nvarchar](20) NOT NULL DEFAULT '',
	[tag] [nvarchar](400) NOT NULL DEFAULT '',
	[active] [bit] NOT NULL DEFAULT 1,
UNIQUE NONCLUSTERED 
(
	[code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Index [metalink_unique]    Script Date: 06/19/2011 00:42:44 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[comdiv].[metalink]') AND name = N'metalink_unique')
DROP INDEX [metalink_unique] ON [comdiv].[metalink] WITH ( ONLINE = OFF )
GO
/****** Object:  Index [metalink_unique]    Script Date: 06/19/2011 00:42:44 ******/
CREATE UNIQUE NONCLUSTERED INDEX [metalink_unique] ON [comdiv].[metalink] 
(
	[code] ASC,
	[srctype] ASC,
	[trgtype] ASC,
	[src] ASC,
	[trg] ASC,
	[type] ASC,
	[subtype] ASC
)
INCLUDE ( [tag],
[active]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [comdiv].[metalink_search]    Script Date: 06/19/2011 00:43:32 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[comdiv].[metalink_search]') AND type in (N'P', N'PC'))
DROP PROCEDURE [comdiv].[metalink_search]
GO
create proc [comdiv].[metalink_search] 
	@code nvarchar(255) = null,
	@srctype nvarchar(50) = null,
	@trgtype nvarchar(50) = null,
	@src nvarchar(50) =  null,
	@trg nvarchar(50) = null,
	@type nvarchar(20) = null,
	@subtype nvarchar(20) = null,
	@tag nvarchar(400) = null,
	@active bit = 1,
	@useactive bit = 1
as begin
	select * from comdiv.metalink
		where 
			(isnull(@tag,'')='' or tag like @tag)
			and
			(isnull(@code,'')='' or code like @code)
			and
			(isnull(@srctype,'')='' or srctype like @srctype)
			and
			(isnull(@trgtype,'')='' or trgtype like @trgtype)
			and
			(isnull(@src,'')=''  or src like @src)
			and
			(isnull(@trg,'')='' or trg like @trg)
			and
			(isnull(@type,'')='' or type like @type)
			and
			(isnull(@subtype,'')='' or subtype like @subtype)
			and
			(@active is null or isnull(@useactive,0)=0  or active = @active)
end
GO

/****** Object:  StoredProcedure [comdiv].[metalink_un_set]    Script Date: 06/19/2011 00:44:21 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[comdiv].[metalink_un_set]') AND type in (N'P', N'PC'))
DROP PROCEDURE [comdiv].[metalink_un_set]
GO
create proc [comdiv].[metalink_un_set]
	@code nvarchar(255) = null ,
	@srctype nvarchar(50) = null,
	@trgtype nvarchar(50) = null,
	@src nvarchar(50) =  null,
	@trg nvarchar(50) = null,
	@type nvarchar(20) = null,
	@subtype nvarchar(20) = null,
	@tag nvarchar(400) = null
as begin
	if @code is null set @code = comdiv.metalink_get_code(@srctype,@trgtype, @src, @trg, @type, @subtype)
	declare @id int set @id = (select id from comdiv.metalink where code = @code)
	if @id is not null begin
		update comdiv.metalink set active = 0 where id = @id
	end
end


GO
/****** Object:  StoredProcedure [comdiv].[metalink_set]    Script Date: 06/19/2011 00:43:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[comdiv].[metalink_set]') AND type in (N'P', N'PC'))
DROP PROCEDURE [comdiv].[metalink_set]
GO
create proc [comdiv].[metalink_set]
	@code nvarchar(255) = null ,
	@srctype nvarchar(50) = null,
	@trgtype nvarchar(50) = null,
	@src nvarchar(50) =  null,
	@trg nvarchar(50) = null,
	@type nvarchar(20) = null,
	@subtype nvarchar(20) = null,
	@tag nvarchar(400) = null,
	@active bit = 1
as begin
	if	@tag = '' set @tag = null
	if @active = 0 begin
		exec comdiv.metalink_un_set @code,@srctype,@trgtype, @src, @trg, @type, @subtype
		return
	end 
	if @code is null set @code = comdiv.metalink_get_code(@srctype,@trgtype, @src, @trg, @type, @subtype)
	declare @id int set @id = (select id from comdiv.metalink where code = @code)
	if @id is null begin
		insert comdiv.metalink(code, srctype, trgtype, src, trg, type, subtype)
		select @code, @srctype, @trgtype, @src, @trg, @type, @subtype
		set @id = scope_identity()
	end
	update comdiv.metalink set tag = isnull(@tag, tag), active = 1 where id = @id
end

GO





/****** Object:  UserDefinedFunction [comdiv].[metalink_get_code]    Script Date: 06/20/2011 13:26:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[comdiv].[metalink_get_code]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [comdiv].[metalink_get_code]
GO


create function [comdiv].[metalink_get_code] (
	@srctype nvarchar(50),
	@trgtype nvarchar(50),
	@src nvarchar(50),
	@trg nvarchar(50),
	@type nvarchar(20),
	@subtype nvarchar(20)
) returns nvarchar(255) as begin
	return dbo.str_format ('{0}_{1}_{2}_{3}_{4}_{5}', @srctype, @src,@type, @subtype, @trgtype, @trg)
end

GO

