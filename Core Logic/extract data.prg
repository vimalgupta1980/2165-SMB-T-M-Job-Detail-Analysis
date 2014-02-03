mbapi = CREATEOBJECT("syscon.masterbuilder")
mbapi.smartGetSMBDir()
progInfo = mbapi.GetProgramInfo()

jobnums = '22417'
strprd = 0
endprd = 12
trandate = DATE(2011, 10, 21)

SafeOpen('jobcst')
SafeOpen('actrec')
SafeOpen('cstcde')
SafeOpen('employ')
SafeOpen('actpay')
SafeOpen('eqpmnt')
SafeOpen('timmat')
SafeOpen('tmemln')
SafeOpen('tmeqln')

* select initial jobcst record list
SELECT ;
		jobcst.recnum ;
		, jobcst.empnum ;
		, jobcst.vndnum ;
		, jobcst.eqpnum ;
		, 000000 as tmemln ;
		, 000000 as tmeqln ;
		, jobnum ;
		, actrec.jobnme ;
		, trnnum ;
		, jobcst.dscrpt ;
		, jobcst.trndte ;
		, jobcst.actprd ;
		, jobcst.srcnum ;
		, jobcst.status ;
		, jobcst.bllsts ;
		, jobcst.phsnum ;
		, jobcst.cstcde ;
		, cstcde.cdenme ;
		, jobcst.csttyp ;
		, jobcst.csthrs ;
		, IIF(jobcst.acrinv = 0 AND jobcst.bllsts = 1 AND jobcst.csttyp = 2 ;
			, jobcst.blgqty ;
			, 000000000) as blgpnd ;
		, jobcst.eqpqty ;
		, jobcst.blgqty ;
		, jobcst.paytyp ;
		, jobcst.cstamt ;
		, jobcst.blgttl ;
		, jobcst.acrinv ;
		, IIF( ;
			jobcst.csttyp = 2, "Employee     ", ; && padding is so that "Equipment" can fit
			IIF(jobcst.csttyp = 3, "Equipment", ;
			IIF(jobcst.empnum <> 0, "Employee", ;
			IIF(jobcst.eqpnum <> 0, "Equipment", ;
			IIF(jobcst.vndnum <> 0, "Vendor", ;
			"None"))))) as empeqpvnd ;
		, PADR('', 100, ' ') as eName ;
		, 00000000.00000000 as rate01 ;
		, 00000000.00000000 as rate02 ;
		, 00000000.00000000 as rate03 ;
		, 00000000.00000000 as minhrs ;
		, 00000001.00000000 as markup ;
		, 000000000000.00000000 as estbll ;
 	FROM jobcst ;
 	JOIN actrec ON jobcst.jobnum = actrec.recnum ;
 	LEFT JOIN cstcde ON cstcde.recnum = jobcst.cstcde ;
	WHERE jobnum in (&jobnums) ;
	AND BETWEEN(actprd, m.strprd, m.endprd) ;
	AND jobcst.status <> 2 ;
	AND trndte <= m.trandate ;
	ORDER BY jobcst.recnum ;
	INTO CURSOR _jobcst READWRITE NOFILTER
	
* set the equipment/employee/vendor name
UPDATE _jobcst SET eName = ALLTRIM(STR(empnum)) + [ - ]  + ALLTRIM(employ.fullst) ;
	from _jobcst ;
	join employ on _jobcst.empnum = employ.recnum ;
	WHERE empeqpvnd = "Employee"
	
UPDATE _jobcst SET eName = ALLTRIM(STR(vndnum)) + [ - ] + ALLTRIM(actpay.vndnme) ;
	from _jobcst ;
	join actpay on _jobcst.vndnum = actpay.recnum ;
	WHERE empeqpvnd = "Vendor"
	
UPDATE _jobcst SET eName = ALLTRIM(STR(eqpnum)) + [ - ] + ALLTRIM(eqpmnt.eqpnme) ;
	from _jobcst ;
	join eqpmnt on _jobcst.eqpnum = eqpmnt.recnum ;
	WHERE empeqpvnd = "Equipment"
	
* find the T&M line item, first find the one with no employee, then replace it if
* proper
UPDATE _jobcst SET _jobcst.tmemln = tmemln.linnum ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	join tmemln on ;
		tmemln.recnum = timmat.emptbl ;
		AND timmat.emptbl <> 0 ;
		AND tmemln.empnum = 0 ;
		AND tmemln.cstcde = _jobcst.cstcde ;
	WHERE _jobcst.csttyp = 2
		
UPDATE _jobcst SET _jobcst.tmemln = tmemln.linnum ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	join tmemln on ;
		tmemln.recnum = timmat.emptbl ;
		AND timmat.emptbl <> 0 ;
		AND tmemln.empnum = _jobcst.empnum ;
		AND tmemln.cstcde = _jobcst.cstcde ;
	WHERE _jobcst.empnum <> 0 AND _jobcst.csttyp = 2
		
* find the proper equipment line items
UPDATE _jobcst SET _jobcst.tmeqln = tmeqln.linnum ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	join tmeqln on tmeqln.recnum = timmat.eqptbl
	
* update the rates, these come from the T&M tables
UPDATE _jobcst SET ;
		_jobcst.rate01 = tmeqln.oprrte ;
		, _jobcst.rate02 = tmeqln.stdrte ;
		, _jobcst.rate03 = tmeqln.idlrte ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	join tmeqln on tmeqln.recnum = timmat.eqptbl AND tmeqln.linnum = _jobcst.tmeqln ;
	WHERE _jobcst.tmeqln <> 0
	
UPDATE _jobcst SET ;
		_jobcst.rate01 = tmemln.rate01 ;
		, _jobcst.rate02 = tmemln.rate02 ;
		, _jobcst.rate03 = tmemln.rate03 ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	join tmemln on tmemln.recnum = timmat.emptbl AND tmemln.linnum = _jobcst.tmemln ;
	WHERE _jobcst.tmemln <> 0
	
* update the minhrs
UPDATE _jobcst SET _jobcst.minhrs = tmeqln.minhrs ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	join tmeqln on tmeqln.recnum = timmat.eqptbl AND tmeqln.linnum = _jobcst.tmeqln ;
	WHERE _jobcst.tmeqln <> 0
	
UPDATE _jobcst SET _jobcst.minhrs = tmemln.minhrs ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	join tmemln on tmemln.recnum = timmat.emptbl AND tmemln.linnum = _jobcst.tmemln ;
	WHERE _jobcst.tmemln <> 0
	
* set the markup
UPDATE _jobcst SET _jobcst.markup = ;
		(1.00 + (timmat.mtrhdn / 100.00)) * ;
		(1.00 + (timmat.mtrshw / 100.00)) * ;
		(1.00 + (timmat.mtrovh / 100.00)) * ;
		(1.00 + (timmat.mtrpft / 100.00)) ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	WHERE _jobcst.csttyp = 1

UPDATE _jobcst SET _jobcst.markup = ;
		(1.00 + (timmat.labhdn / 100.00)) * ;
		(1.00 + (timmat.labshw / 100.00)) * ;
		(1.00 + (timmat.labovh / 100.00)) * ;
		(1.00 + (timmat.labpft / 100.00)) ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	WHERE _jobcst.csttyp = 2
	
UPDATE _jobcst SET _jobcst.markup = ;
		(1.00 + (timmat.eqphdn / 100.00)) * ;
		(1.00 + (timmat.eqpshw / 100.00)) * ;
		(1.00 + (timmat.eqpovh / 100.00)) * ;
		(1.00 + (timmat.eqppft / 100.00)) ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	WHERE _jobcst.csttyp = 3
	
UPDATE _jobcst SET _jobcst.markup = ;
		(1.00 + (timmat.subhdn / 100.00)) * ;
		(1.00 + (timmat.subshw / 100.00)) * ;
		(1.00 + (timmat.subovh / 100.00)) * ;
		(1.00 + (timmat.subpft / 100.00)) ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	WHERE _jobcst.csttyp = 4
	
UPDATE _jobcst SET _jobcst.markup = ;
		(1.00 + (timmat.otrhdn / 100.00)) * ;
		(1.00 + (timmat.otrshw / 100.00)) * ;
		(1.00 + (timmat.otrovh / 100.00)) * ;
		(1.00 + (timmat.otrpft / 100.00)) ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	WHERE _jobcst.csttyp = 5
	
UPDATE _jobcst SET _jobcst.markup = ;
		(1.00 + (timmat.cs6hdn / 100.00)) * ;
		(1.00 + (timmat.cs6shw / 100.00)) * ;
		(1.00 + (timmat.cs6ovh / 100.00)) * ;
		(1.00 + (timmat.cs6pft / 100.00)) ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	WHERE _jobcst.csttyp = 6
	
UPDATE _jobcst SET _jobcst.markup = ;
		(1.00 + (timmat.cs7hdn / 100.00)) * ;
		(1.00 + (timmat.cs7shw / 100.00)) * ;
		(1.00 + (timmat.cs7ovh / 100.00)) * ;
		(1.00 + (timmat.cs7pft / 100.00)) ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	WHERE _jobcst.csttyp = 7
	
UPDATE _jobcst SET _jobcst.markup = ;
		(1.00 + (timmat.cs8hdn / 100.00)) * ;
		(1.00 + (timmat.cs8shw / 100.00)) * ;
		(1.00 + (timmat.cs8ovh / 100.00)) * ;
		(1.00 + (timmat.cs8pft / 100.00)) ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	WHERE _jobcst.csttyp = 8
	
UPDATE _jobcst SET _jobcst.markup = ;
		(1.00 + (timmat.cs9hdn / 100.00)) * ;
		(1.00 + (timmat.cs9shw / 100.00)) * ;
		(1.00 + (timmat.cs9ovh / 100.00)) * ;
		(1.00 + (timmat.cs9pft / 100.00)) ;
	from _jobcst ;
	join timmat on timmat.recnum = _jobcst.jobnum ;
	WHERE _jobcst.csttyp = 9

* get the billing quantities	
SELECT _jobcst.recnum, 00000000.00000000 as hrs, CAST(null as n(3)) as typ FROM _jobcst INTO CURSOR blgqty READWRITE NOFILTER

UPDATE blgqty SET hrs = _jobcst.csthrs, typ = _jobcst.paytyp ;
	from blgqty ;
	join _jobcst on blgqty.recnum = _jobcst.recnum ;
	WHERE _jobcst.csttyp = 2
	
UPDATE blgqty SET hrs = _jobcst.blgqty, typ = _jobcst.paytyp ;
	from blgqty ;
	join _jobcst on blgqty.recnum = _jobcst.recnum ;
	WHERE _jobcst.csttyp = 2 AND blgqty.hrs = 0 AND _jobcst.bllsts = 1
	
UPDATE blgqty SET hrs = _jobcst.eqpqty, typ = eqpmnt.eqptyp ;
	from blgqty ;
	join _jobcst on blgqty.recnum = _jobcst.recnum ;
	join eqpmnt on _jobcst.eqpnum = eqpmnt.recnum ;
	WHERE _jobcst.csttyp = 3 AND _jobcst.eqpnum <> 0 AND eqpmnt.eqptyp <> 0
	
* update billing amounts
UPDATE _jobcst SET _jobcst.estbll = ;
		IIF(typ = 1 AND rate01 <> 0, hrs*rate01, ;
		IIF(typ = 2 AND rate02 <> 0, hrs*rate02, ;
		IIF(typ = 3 AND rate03 <> 0, hrs*rate03, ;
		cstamt))) ;
	from _jobcst ;
	join blgqty on blgqty.recnum = _jobcst.recnum
	
UPDATE _jobcst SET estbll = cstamt ;
	from _jobcst ;
	join eqpmnt on eqpmnt.recnum = _jobcst.eqpnum ;
	WHERE _jobcst.csttyp = 3 AND (ISNULL(eqpmnt.eqptyp) or eqpmnt.eqptyp = 0)
	

* markup estimated billing
UPDATE _jobcst SET estbll = estbll * markup
	
* if it's already been billed, or cannot be billed, set estbll correct
UPDATE _jobcst SET estbll = 0 WHERE bllsts = 2

UPDATE _jobcst SET estbll = blgttl WHERE bllsts = 3

** TODO: set estbll to 0 for non-billable cost codes
UPDATE _jobcst SET estbll = 0 WHERE cstcde < 1000
	
SELECT _jobcst
BROWSE nowait

FUNCTION SafeOpen
	LPARAMETERS tblName
	
	USE (progInfo.SMBDir + [\] + tblName) SHARED AGAIN IN 0
ENDFUNC