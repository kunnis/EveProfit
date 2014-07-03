
drop table buyorders
drop table sellorders

create table sellorders (
	itemid int not null,
	orderid bigint not null,
	stationid int not null,
	price money not null,
	volume int not null,
	minvolume int not null,
)

create table buyorders (
	itemid int not null,
	orderid bigint not null,
	stationid int not null,
	price money not null,
	volume int not null,
	minvolume int not null,
)

ALTER TABLE dbo.sellorders ADD CONSTRAINT
	CI_sellorders UNIQUE CLUSTERED 
	(
	itemid,
	stationid,
	orderid
	)
	
ALTER TABLE dbo.buyorders ADD CONSTRAINT
	CI_buyorders UNIQUE CLUSTERED 
	(
	itemid,
	stationid,
	orderid
	)


ALTER TABLE dbo.buyorders ADD
	timestamp datetime NOT NULL CONSTRAINT DF_buyorders_timestamp DEFAULT getutcdate()
ALTER TABLE dbo.sellorders ADD
	timestamp datetime NOT NULL CONSTRAINT DF_sellorders_timestamp DEFAULT getutcdate()

ALTER TABLE dbo.buyorders ADD
	range int NOT NULL CONSTRAINT DF_buyorders_range DEFAULT -10
ALTER TABLE dbo.sellorders ADD
	range int NOT NULL CONSTRAINT DF_sellorders_range DEFAULT -10