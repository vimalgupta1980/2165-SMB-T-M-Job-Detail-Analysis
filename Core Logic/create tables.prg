SET SAFETY OFF

CREATE TABLE [Blank Tables\syscon_tm_log] ( ;
	recnum n(10,0) NOT NULL, ;
	usrnme c(20) NOT NULL, ;
	chgdsc c(200) NOT NULL, ;
	chgdte date NOT NULL, ;
	chgpth memo NOT NULL ;
)
	