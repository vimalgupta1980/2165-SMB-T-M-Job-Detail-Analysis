* Calculates unbilled time in job costs for T&M jobs
* Version 04 TODO LIST
*	Break out the allocations from non-billable cost records as a separate column
* Version 03
*	History
*		2/11/10 added summary by project manager of unbilled costs.  This includes only costs that
*				are marked as non-billable, not costs pending billing.
* Version 04
*	6/2/10	Added informational documentation to assist in the incorporation of this logic into the 
*			over/under billing program (OUBP) version 1.3.6
* Version 05
*	7/1/10	Add initial job filter 
*			Added logic for employee specific billing rates
*			Added logic for equipment costs based on equipment wage rates
*			Various queries fixed:  Search code for "Version 5"
* Version 06 
* Version 07
*	12/28/10	Added jobcostfilter option
*				Added the collection of all job cost detail into alljobdetail
* 
* Version 08  (Search for "Version 8" to locate modifications)
*   02/17/11	Added logic to count any records that were billed in a subsequent period 
*				as billable in the analysis period.
*				Corrected issue with cs9pft is a non-numeric field in MB

SET safety off 
SET DELETED ON 
CLOSE TABLES
SET CONFIRM ON 
SET TALK OFF 
CLEAR 


PUBLIC datadir, startperiod, endperiod, nonbillcstcde[10], exportdetail, fileroot


* The following fields will be taken from the run-time parameters of the OUBP
datadir = "x:\mb7\aps\"	&& This is the source data set and must match the data directory selected in the OUBP
exportdetail = .f. 		&& Indicates whether to export detailed job reports to Excel for each job analyzed
						&&   This parameter will be used as an internal debugging parameter only and should always be false
						&& 	 for the production version of the program
startperiod = 2			&& 	Use the setting already in place in the OUBP
endperiod = 2			&& 	Use the setting already in place in the OUBP

* jobfilter = "inlist(actrec.recnum,526592)"		&& When incorporated into the production version of OUBP
													&& this filter should be the same as the same as the filter that
													&& controls the job selection for the over/under review sheet.
jobfilter = ".t."
jobtypefilter = ".t."

* jobstatusfilter = "inlist(actrec.status,4)"
jobstatusfilter = ".t."

* Jobcostfilter = " jobcst.csttyp = 1 and AT('MATERIALS',upper(jobcst.dscrpt)) > 0 and jobcst.acrinv > 0 "
* Jobcostfilter = " jobcst.csttyp = 1 and jobcst.acrinv > 0 "
* Jobcostfilter = " jobcst.csttyp = 1 "
jobcostfilter = ".t."

* This is the file that is created by this code fragment.   When incorporated in the OUBP, this data will be 
* exported to the user selected template and this file will not be created.
fileroot = "result files\APS 2010 Period " + ALLTRIM(STR(startperiod,2,0)) + " to " + ;
	ALLTRIM(STR(endperiod,2,0)) + " - "

* Non-billed cost codes are cost codes that have been designated as non-billable and should not be included in 
* the T&M analysis.

* In the T&M analysis setup screen, select the list of cost codes for the data directory (cstcde) and allow the user 
* to mark the cost codes that are non-billed cost codes.   The results of these user selections should be maintained 
* in a database in the currently selected SMB data directory in a file named "sys_oubp_xxxxxx" where xxxxxxx can be any file 
* descriptor required.
FOR i = 1 TO 10
	nonbillcstcde[i] = 0
NEXT i
nonbillcstcde[1] = 1
nonbillcstcde[2] = 2
nonbillcstcde[3] = 79
nonbillcstcde[4] = 89
nonbillcstcde[5] = 99
nonbillcstcde[6] = 900

USE (datadir + "actrec") SHARED IN 0 NOUPDATE
USE (datadir + "acrinv") SHARED IN 0 NOUPDATE
USE (datadir + "arivln") SHARED IN 0 noupdate 
USE (datadir + "jobcst") SHARED IN 0 noupdate
USE (datadir + "jobtyp") SHARED IN 0 NOUPDATE
USE (datadir + "timmat") SHARED IN 0 NOUPDATE 
USE (datadir + "timeqp") SHARED IN 0 NOUPDATE 
USE (datadir + "tmemln") SHARED IN 0 NOUPDATE 
USE (datadir + "tmeqln") SHARED IN 0 noupdate
USE (datadir + "cstcde") SHARED IN 0 noupdate
USE (datadir + "employ") SHARED IN 0 noupdate
USE (datadir + "eqpmnt") SHARED IN 0 noupdate


* Initial query to get the jobs to be included in the T&M analysis.   This filter 
* preceeds the filter on T&M job type with the subseqent query for detailed job information
SELECT actrec.recnum FROM actrec WHERE &jobfilter ;
	AND &jobtypefilter ;
	AND &jobstatusfilter ;
	INTO CURSOR jobselection 

* This query will need to be modified when incorporated into OUBP:
*	The value selected for estgp will be taken from a user defined field that may or may not be numeric
*	The job types that are considered TM will be selected from a user maintained database
*	
* Get the list of jobs to test - determine which ones are T&M and which are not
* Version 5: removed filter on actrec.status and added join to jobselection for this query
SELECT actrec.recnum as jobnum, actrec.lotprm as estgp, actrec.sprvsr as sprvsr, jobtyp.recnum as jobtyp, jobtyp.typnme, ;
	IIF(UPPER(SUBSTR(jobtyp.typnme,1,2))="TM", "TM", "OTHER") as jobtype, ;
	0000000.00 as ttllabhrs, 00000000.00 as actualAR, 000000000.00 as bill01, ;
	000000000.00 as cost01, 000000000.00 as nobill01, 000000000.00 as bill01cp, ;
	000000000.00 as cost01cp, 000000000.00 as nobill01cp, ;
	000000000.00 as bill02, 000000000.00 as cost02, ;
	000000000.00 as bill03, 000000000.00 as cost03, ;
	000000000.00 as estbilling, 000000000.00 as ttlcst, ;
	"   " as tmtable, 0000000000 as count03, 0000000000 as count03ar, ;
	000000000.00 as hours01, 000000000.00 as hours02, 000000000.00 as hours03, ;
	000000000.00 as hours03b, 000000000.00 as hours04 ;
	FROM actrec JOIN jobtyp ON actrec.jobtyp = jobtyp.recnum ;
	JOIN jobselection ON actrec.recnum = jobselection.recnum ;
	INTO CURSOR joblist READWRITE 

SELECT joblist.jobnum, NVL((select SUM(csthrs) from jobcst WHERE jobcst.jobnum = joblist.jobnum ;
					AND jobcst.status <> 2 AND jobcst.actprd <= endperiod AND &jobcostfilter),00000000.00) as ttlhours ;
					FROM joblist INTO CURSOR jobhours READWRITE 
					
UPDATE jobhours SET ttlhours = 0 WHERE ISNULL(ttlhours)

SELECT joblist.jobnum, (SELECT SUM(invttl) FROM acrinv WHERE acrinv.jobnum = joblist.jobnum ;
					AND acrinv.status < 5 AND acrinv.actper <= endperiod) as totalAR ;
					FROM joblist INTO CURSOR ARbilling READWRITE 

UPDATE ARbilling SET totalAR = 0 WHERE ISNULL(totalAR)

* Compute the estimated unbilled time for each job in job list that is indicated as a T&M job
SELECT joblist
SCAN
	IF joblist.jobtype = "TM"
		DO estunbilled WITH joblist.jobnum
		SELECT costs
	ENDIF 
	SELECT joblist
ENDSCAN 

* Update the joblist with summary information
* This is the total hours on the job as per the job cost records
UPDATE joblist SET ;
	ttllabhrs = jobhours.ttlhours ;
	from jobhours WHERE joblist.jobnum = jobhours.jobnum

* The actual billing on the job
UPDATE joblist SET ;
	actualar = arbilling.totalAR ;
	from arbilling WHERE joblist.jobnum = arbilling.jobnum


UPDATE joblist SET ;
	joblist.bill01 = costsummary.bill01, ;
	joblist.cost01 = costsummary.cost01, ;
	joblist.nobill01 = costsummary.nobill01, ;
	joblist.bill01cp = costsummary.bill01cp, ;
	joblist.cost01cp = costsummary.cost01cp, ;
	joblist.nobill01cp = costsummary.nobill01cp, ;
	joblist.bill02 = costsummary.bill02, ;
	joblist.cost02 = costsummary.cost02, ;
	joblist.bill03 = costsummary.bill03, ;
	joblist.cost03 = costsummary.cost03, ;
	joblist.estbilling = costsummary.estbilling, ;
	joblist.ttlcst = costsummary.cstamt, ;
	joblist.tmtable = costsummary.tmtable, ;
	joblist.count03 = costsummary.count03, ;
	joblist.count03ar = costsummary.count03ar, ;
	joblist.hours01 = costsummary.hours01, ;
	joblist.hours02 = costsummary.hours02, ;
	joblist.hours03 = costsummary.hours03, ;
	joblist.hours03b = costsummary.hours03b, ;
	joblist.hours04 = costsummary.hours04 ;
	from costsummary WHERE joblist.jobnum = costsummary.jobnum

* The cursor joblist contains the data that will be pushed into the OUBP 
SELECT joblist.* FROM joblist INTO CURSOR joblist ORDER BY jobnum
SELECT joblist
COPY TO (fileroot + "Detailed T&M Analysis") TYPE xl5

* Get the hours billed analysis
SELECT jobnum, sprvsr, employ.fullst, jobtyp, typnme, ;
	ttllabhrs, hours01, hours02, hours03, hours03b, hours04 ;
	FROM joblist LEFT JOIN employ ON joblist.sprvsr = employ.recnum ;
	WHERE jobtype = "TM" ;
	INTO CURSOR hourslist READWRITE 

SELECT sprvsr, MAX(fullst) as fullst, SUM(ttllabhrs) as ttllabhrs, ;
	SUM(hours01) as hours01, SUM(hours02) as hours02, SUM(hours03) as hours03, ;
	SUM(hours03b) as hours03b, SUM(hours04) as hours04 ;
	FROM hourslist INTO CURSOR hourssum READWRITE GROUP BY 1

* The cursor hours list contains data that will optionally be pushed into the OUBP 
SELECT hourslist
COPY TO (fileroot + "Detailed Job Hours Analysis") TYPE xl5

* The cursor hours list contains data that will optionally be pushed into the OUBP 
SELECT hourssum 
COPY TO (fileroot + "Summary Job Hours Analysis by PM") TYPE xl5

* Write out the all job detail
SELECT alljobdetail
COPY TO (fileroot + "All Job Detailed Analysis") TYPE csv

RETURN 

*------------------------------------------------------------------------------------*
PROCEDURE estunbilled 
*------------------------------------------------------------------------------------*
PARAMETERS jobnumber


* Get the total billing information from job costs for this period and prior
filename = "result files\Job Detail " + STR(jobnumber, 7,0)
tmtable = "YES"

* Determine if there is a T&M table for this job.
SELECT timmat.* FROM timmat WHERE recnum = jobnumber INTO CURSOR tmrates 

SELECT tmrates
IF reccount('tmrates') = 0
	? "No T&M table for this job ", jobnumber
	tmtable = "NO"
ENDIF 
 

? "Computing job", jobnumber
* Get all the billing information from job cost
SELECT jobcst.* ;
	FROM jobcst ;
	WHERE jobnum = jobnumber AND status <> 2 AND actprd <= endperiod ;
	AND &jobcostfilter ;
	INTO CURSOR allcosts READWRITE NOFILTER 

* Get the Accounting Period for the billing invoice  (Version 08)
SELECT allcosts.*, NVL(acrinv.actper,00) as acrprd ;
	FROM allcosts LEFT JOIN acrinv ON allcosts.acrinv = acrinv.recnum ;
	INTO CURSOR allcosts READWRITE NOFILTER 
	
* Reverse billing status for any job costs that were billed AFTER the selected analysis period. 
* With this, the open job cost records will be reflected properly as of the end of the analysis period.
UPDATE allcosts SET bllsts = 1 WHERE bllsts = 3 AND acrprd > m.endperiod


* Determine the expected billing on each item
* First get the cost code specific rate for each - this is the default
* Modified with Version 5
* Note that I have added a place holder for eqptype which is intended for the equipment type as 
*  defined in the 8-3 equipment screen.  The field eqptyp in job costing refers to the type of work
*  being performed by the eqiupment - operated, standby, idle - much like paytyp refers to the
*  type of hours in a payroll record.
SELECT allcosts.*, tmemln.rate01, tmemln.rate02, tmemln.rate03, tmemln.minhrs, ;
	00000.00000000 as markup, 0000000000.00 as estbilling, ;
	tmrates.emptbl as emptbl, tmrates.eqptbl as eqptbl, 000 as eqptype FROM allcosts ;
	LEFT JOIN tmemln ON ;
		allcosts.cstcde = tmemln.cstcde AND tmemln.recnum = tmrates.emptbl ;
		AND EMPTY(tmemln.empnum) AND allcosts.csttyp = 2;
		INTO CURSOR costs readwrite 

* Update the rates that are employee/cost codes specific if there are any
* Added with Version 5
UPDATE costs SET ;
	rate01 = tmemln.rate01, rate02 = tmemln.rate02, rate03 = tmemln.rate03, ;
	minhrs = tmemln.minhrs 	;
	from tmemln ;
	WHERE costs.emptbl = tmemln.recnum AND costs.cstcde = tmemln.cstcde ;
		AND costs.empnum = tmemln.empnum

* Update the costs if they are equipment costs
* Added with Version 5
* First, get the equipment type for each job cost record with equipment number
UPDATE costs SET costs.eqptype = eqpmnt.eqptyp from eqpmnt ;
	WHERE costs.eqpnum = eqpmnt.recnum 
* Now, get the default costs by equipment type only
UPDATE costs SET ;
	rate01 = tmeqln.oprrte, rate02 = tmeqln.stdrte, rate03 = tmeqln.idlrte, ;
	minhrs = tmeqln.minhrs 	;
	from tmeqln ;
	WHERE costs.eqptbl = tmeqln.recnum AND costs.eqptype = tmeqln.eqptyp ;
		AND EMPTY(tmeqln.eqpnum)
* Finally, get any rates that are specific to a piece of equipment
UPDATE costs SET ;
	rate01 = tmeqln.oprrte, rate02 = tmeqln.stdrte, rate03 = tmeqln.idlrte, ;
	minhrs = tmeqln.minhrs 	;
	from tmeqln ;
	WHERE costs.eqptbl = tmeqln.recnum AND costs.eqptype = tmeqln.eqptyp ;
		AND costs.eqpnum = tmeqln.eqpnum


* Get the mark ups based on cost type
SELECT costs
SCAN 
	tvalue = 1
	DO case
		CASE costs.csttyp = 1
			tvalue = (1+TMRates.mtrhdn/100) * (1+TMRates.mtrshw/100) * (1+TMRates.mtrovh/100) * (1+TMRates.mtrpft/100)
		CASE costs.csttyp = 2
			tvalue = (1+TMRates.labhdn/100) * (1+TMRates.labshw/100) * (1+TMRates.labovh/100) * (1+TMRates.labpft/100)
		CASE costs.csttyp = 3
			tvalue = (1+TMRates.eqphdn/100) * (1+TMRates.eqpshw/100) * (1+TMRates.eqpovh/100) * (1+TMRates.eqppft/100)
		CASE costs.csttyp = 4
			tvalue = (1+TMRates.subhdn/100) * (1+TMRates.subshw/100) * (1+TMRates.subovh/100) * (1+TMRates.subpft/100)
		CASE costs.csttyp = 5
			tvalue = (1+TMRates.otrhdn/100) * (1+TMRates.otrshw/100) * (1+TMRates.otrovh/100) * (1+TMRates.otrpft/100)
		CASE costs.csttyp = 6
			tvalue = (1+TMRates.cs6hdn/100) * (1+TMRates.cs6shw/100) * (1+TMRates.cs6ovh/100) * (1+TMRates.cs6pft/100)
		CASE costs.csttyp = 7
			tvalue = (1+TMRates.cs7hdn/100) * (1+TMRates.cs7shw/100) * (1+TMRates.cs7ovh/100) * (1+TMRates.cs7pft/100)
		CASE costs.csttyp = 8
			tvalue = (1+TMRates.cs8hdn/100) * (1+TMRates.cs8shw/100) * (1+TMRates.cs8ovh/100) * (1+TMRates.cs8pft/100)
		CASE costs.csttyp = 9
			tvalue = (1+TMRates.cs9hdn/100) * (1+TMRates.cs9shw/100) * (1+TMRates.cs9ovh/100) * (1+VAL(TMRates.cs9pft)/100)
	endcase
	* Determine the estimated billing
	tbilling = 0
	nonbillcode = 0
	DO CASE 
		CASE bllsts = 3		&& This item has been billed, so there is no need to estimate the billing on it
			tbilling = costs.blgttl
		CASE bllsts = 2		&& This item has been marked so it should not be billed.
			tbilling = 0
		OTHERWISE 
			DO CASE 
				CASE costs.csttyp = 2 AND costs.csthrs <> 0			&& Labor - Get the wage multiplier
					DO CASE 
						CASE ISNULL(costs.rate01) OR ISNULL(costs.rate02) OR ISNULL(costs.rate03)
							tbilling = costs.cstamt
						CASE costs.paytyp = 1
							tbilling = costs.csthrs * costs.rate01
						CASE costs.paytyp = 2
							tbilling = costs.csthrs * costs.rate02
						CASE costs.paytyp = 3
							tbilling = costs.csthrs * costs.rate03
						OTHERWISE 									&& Another type of pay
							tbilling = costs.cstamt
					ENDCASE 
				CASE costs.csttyp = 3 AND costs.eqpqty <> 0			&& Equipment - get the multiplier
					DO CASE 
						CASE ISNULL(costs.rate01) OR ISNULL(costs.rate02) OR ISNULL(costs.rate03)
							tbilling = costs.cstamt
						CASE costs.eqptyp = 1
							tbilling = costs.eqpqty * costs.rate01
						CASE costs.eqptyp = 2
							tbilling = costs.eqpqty * costs.rate02
						CASE costs.eqptyp = 3
							tbilling = costs.eqpqty * costs.rate03
						OTHERWISE 									&& Another type of pay
							tbilling = costs.cstamt
					ENDCASE 
				OTHERWISE 
					tbilling = costs.cstamt
			ENDCASE 
			* Multiplier by mark up percentage
			tbilling = tbilling*tvalue
			* Remove the estimated billing if it is a non-billed cost code
			IF ASCAN(nonbillcstcde, costs.cstcde) > 0
				tbilling = 0
				nonbillcode = 1
			ENDIF 
	ENDCASE 
	
	* Identify that there are nonbilling codes in here if appropriate for later compilation
	replace costs.markup WITH tvalue, ;
			costs.estbilling WITH tbilling, ;
			costs.bllsts WITH IIF(nonbillcode = 1,4,costs.bllsts)

ENDSCAN 

* Add acrprd for Version 8
SELECT j.recnum, j.jobnum, j.trnnum, j.dscrpt, j.trndte, j.actprd, j.srcnum, j.status, j.bllsts, j.phsnum, ;
	j.cstcde, j.csttyp, j.csthrs, j.paytyp, j.cstamt, j.blgttl, j.acrinv, j.rate01, j.rate02, ;
	j.rate03, j.minhrs, j.markup, j.estbilling ;
	FROM costs j ;
	INTO CURSOR costtemp ORDER BY trndte READWRITE NOFILTER 

IF RECCOUNT('costtemp') > 0 AND exportdetail 
	SELECT costtemp
	COPY TO (filename) TYPE xls
ENDIF 

* Collect the detail for all jobs
IF SELECT('alljobdetail') = 0
	SELECT * FROM costtemp INTO CURSOR alljobdetail READWRITE NOFILTER 
ELSE
	INSERT INTO alljobdetail SELECT * FROM costtemp
ENDIF 
	


IF SELECT('costsummary') = 0
	CREATE TABLE costsummary ( ;
		jobnum	n(15,0), ;
		bill01	n(15,2), ;
		cost01	n(15,2), ;
		nobill01	n(15,2), ;
		bill01cp	n(15,2), ;
		cost01cp	n(15,2), ;
		nobill01cp	n(15,2), ;
		bill02	n(15,2), ;
		cost02	n(15,2), ;
		bill03	n(15,2), ;
		cost03	n(15,2), ;
		estbilling	n(15,2), ;
		cstamt	n(15,2), ;
		tmtable	c(3), ;
		count03	n(10,0), ;
		count03ar	n(10,0), ;
		hours01		n(15,2), ;
		hours02		n(15,2), ;
		hours03		n(15,2), ;
		hours03b	n(15,2), ;
		hours04		n(15,2) )
ENDIF 


* Version 5: This query was modif
INSERT INTO costsummary (jobnum, bill01, cost01, nobill01, bill01cp, cost01cp, nobill01cp, ;
		bill02, cost02, bill03, cost03, estbilling, cstamt, tmtable, count03, count03ar, ;
		hours01, hours02, hours03, hours03b, hours04 ) ;
	SELECT jobnum, ;
		SUM(IIF(bllsts=1,estbilling,0)) as bill01, ;
		SUM(IIF(bllsts=1,cstamt,0)) as cost01, ;
		SUM(IIF(bllsts=4,cstamt,0)) as nobill01, ;
		SUM(IIF(bllsts=1 AND BETWEEN(actprd,startperiod,endperiod),estbilling,0)) as bill01cp, ;
		SUM(IIF(bllsts=1 AND BETWEEN(actprd,startperiod,endperiod),cstamt,0)) as cost01cp, ;
		SUM(IIF(bllsts=4 AND BETWEEN(actprd,startperiod,endperiod),cstamt,0)) as nobill01cp, ;
		SUM(IIF(bllsts=2,estbilling,0)) as bill02, ;
		SUM(IIF(bllsts=2,cstamt,0)) as cost02, ;
		SUM(IIF(bllsts=3,estbilling,0)) as bill03, ;
		SUM(IIF(bllsts=3,cstamt,0)) as cost03, ;
		SUM(estbilling) as estbilling, ;
		SUM(cstamt) as cstamt, m.tmtable as tmtable, ;
		sum(IIF(bllsts=3,1,0)) as count03, ;
		sum(IIF(acrinv > 0, 1,0)) as count03ar, ;
		SUM(IIF(bllsts=1,csthrs,0)) as hours01, ;
		SUM(IIF(bllsts=2,csthrs,0)) as hours02, ;
		SUM(IIF(bllsts=3,csthrs,0)) as hours03, ;
		SUM(IIF(bllsts=3,blgqty,0)) as hours03b, ;
		SUM(IIF(bllsts=4,csthrs,0)) as hours04 ;
		FROM costs GROUP BY 1
	
* Determine how much of the actual billing 


RETURN 

