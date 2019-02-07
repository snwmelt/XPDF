/*
	==========================================================================
        Module: ADBC.js
	==========================================================================
        JavaScript constants for ADBC.
	==========================================================================
	The Software, including this file, is subject ot the End User License
	Agreement.
	Copyright (c) 1997, Adobe Systems Incorporated, All Rights Reserved.
	==========================================================================
*/

// The following code "exports" any strings in the list into the current scope.
if(typeof ADBC != "undefined")
{
	var adbcStrsToExport =["IDS_ADBC_CONSOLEMSG_OK"];
 
	for(var n = 0; n < adbcStrsToExport.length; n++)
		eval(adbcStrsToExport[n] + " = " + app.getString("ADBC", adbcStrsToExport[n]).toSource());

	console.println(IDS_ADBC_CONSOLEMSG_OK);

	// SQL types

	ADBC.SQLT_BIGINT = 0;
	ADBC.SQLT_BINARY = 1;
	ADBC.SQLT_BIT = 2;
	ADBC.SQLT_CHAR = 3;
	ADBC.SQLT_DATE = 4;
	ADBC.SQLT_DECIMAL = 5;
	ADBC.SQLT_DOUBLE = 6;
	ADBC.SQLT_FLOAT = 7;
	ADBC.SQLT_INTEGER = 8;
	ADBC.SQLT_LONGVARBINARY = 9;
	ADBC.SQLT_LONGVARCHAR = 10;
	ADBC.SQLT_NUMERIC = 11;
	ADBC.SQLT_REAL = 12;
	ADBC.SQLT_SMALLINT = 13;
	ADBC.SQLT_TIME = 14;
	ADBC.SQLT_TIMESTAMP = 15;
	ADBC.SQLT_TINYINT = 16;
	ADBC.SQLT_VARBINARY = 17;
	ADBC.SQLT_VARCHAR = 18;

	// SQL type to string map

	ADBC.SQLTStrings = new Array(
		"BIGINT",
		"BINARY",
		"BIT",
		"CHAR",
		"DATE",
		"DECIMAL",
		"DOUBLE",
		"FLOAT",
		"INTEGER",
		"LONGVARBINARY",
		"LONGVARCHAR",
		"NUMERIC",
		"REAL",
		"SMALLINT",
		"TIME",
		"TIMESTAMP",
		"TINYINT",
		"VARBINARY",
		"VARCHAR"
	);

	// JavaScript type identifiers

	ADBC.Stream = 0x10000; // this is a flag!  or it with any of the following types
	ADBC.Numeric = 0;
	ADBC.String = 1;
	ADBC.Binary = 2;
	ADBC.Boolean = 3;
	ADBC.Time = 4;
	ADBC.Date = 5;
	ADBC.TimeStamp = 6;

	// Returns an array containing each column with the value property in best-fit format
	// Used internally by the Statement object to implement XXX.getColumnArray()
	function ADBCGetColumnArrayFromStatement(s)
	{
	  var aRet = new Array(s.columnCount + 1);
	  var n;

	  for(n = 1; n < s.columnCount + 1; n++)
		aRet[n] = s.getColumn(n);
	  return aRet;
	}

	// Returns an object containing a property for each column whose name is a
	//   valid property name
	// Value properties of each column are in best-fit format
	// Property columnArray contains an array that contains the return value of
	//   ADBCGetColumnArrayFromStatement
	// Used internally by the Statement object to implement XXX.getRow()
	function ADBCGetRowFromStatement(s)
	{
	  var r = {};
	  var a = ADBCGetColumnArrayFromStatement(s);
	  var n;

	  for(n = 1; n < a.length; n++)
		r[a[n].name] = a[n];
	  r.columnArray = a;
	  return r;
	}
}