if(typeof Collab != "undefined")
{
	var strsToExport =[
		"IDS_SUM_TITLE1",
		"IDS_SUM_TITLE2",
		"IDS_UNNAMED",
		"IDS_SUM_DATE1",
		"IDS_SUM_DATE2",
		"IDS_SUM_AUTHOR1",
		"IDS_SUM_AUTHOR2",
		"IDS_SUM_SUBJ1",
		"IDS_SUM_SUBJ2",
		"IDS_SUM_LABEL1",
		"IDS_SUM_LABEL2",
		"IDS_SUM_PAGE1",
		"IDS_SUM_PAGE2",
		"IDS_SUM_TYPE1",
		"IDS_SUM_TYPE2",
		"IDS_SUM_SEQ1",
		"IDS_SUM_SEQ2",
		"IDS_SUM_NO_ANNOTS1",
		"IDS_SUM_NO_ANNOTS2",
		"IDS_STORE_WEB_DISCUSSIONS",
		"IDS_STORE_DAVFDF",
		"IDS_STORE_FSFDF",
		"IDS_STORE_DATABASE",
		"IDS_STORE_NONE",
		"IDS_PROGRESS_SUMMARIZE",
		"IDS_PROGRESS_SORTING",
		"IDS_PROGRESS_FETCHING",
		"IDS_PROGRESS_FETCHING_BIG",
		"IDS_PROGRESS_ADDING",
		"IDS_PROGRESS_DELETING",
		"IDS_PROGRESS_CHANGING",
		"IDS_ANNOTS_JS_BUILTIN",
		"IDS_DATE_INDETERMINATE"
	];

	for(var n = 0; n < strsToExport.length; n++)
	{
		var strID = strsToExport[n];

		eval(strID + " = " + app.getString("Annots", strID).toSource());
	}

	console.println(IDS_ANNOTS_JS_BUILTIN);

	/* for debugging */
	function debugExcept(e)
	{
		if((typeof app._DEBUG != "undefined") && app._DEBUG)
		  console.println(e)
	}

	/* Sort methods */
	ANSB_None = 0;
	ANSB_Page = 1;
	ANSB_Seq = 2;
	ANSB_Author = 3;
	ANSB_ModDate = 4;
	ANSB_Type = 5;

	ANFB_ShouldPrint = 0;
	ANFB_ShouldView = 1;
	ANFB_ShouldEdit = 2;
	ANFB_ShouldAppearInPanel = 3;
	ANFB_ShouldSummarize = 4;
	ANFB_ShouldExport = 5;
	ANFB_ShouldNone = 6;

	/* Field to summary functions by property name */
	ANsums =
	[
	/* None */		function(a){return "*None*";},
	/* Page */		function(a){return IDS_SUM_PAGE1+a.doc.getPageLabel(a.page)+IDS_SUM_PAGE2;},
	/* Sequence */	function(a){return IDS_SUM_SEQ1+a.seqNum+IDS_SUM_SEQ2;},
	/* Author */	function(a){return IDS_SUM_AUTHOR1+a.author+IDS_SUM_AUTHOR2;},
	/* ModDate */	function(a){
		var d = a.modDate; 
		return IDS_SUM_DATE1+ (d ? util.printd(2, a.modDate) : IDS_DATE_INDETERMINATE )+IDS_SUM_DATE2;
		},
	/* Type */		function(a){return IDS_SUM_TYPE1+a.uiType+IDS_SUM_TYPE2;},
	];

	/* Order of summary fields */
	ANsumorder = [ ANSB_Page, ANSB_Seq, ANSB_Author, ANSB_ModDate, ANSB_Type ];

	/* binary insertion into sorted list */
	function binsert(a, m)
	{
		var nStart = 0, nEnd = a.length - 1;

		while(nStart < nEnd)
		{
			var nMid = Math.floor((nStart + nEnd) / 2);

			if(m.toString() < a[nMid].toString())
				nEnd = nMid - 1;
			else
				nStart = nMid + 1;
		}
		if((nStart < a.length) && (m.toString() >= a[nStart].toString()))
			a.splice(nStart + 1, 0, m);
		else
			a.splice(nStart, 0, m);
	}

	/* perform a worst case n log ( n ) sort with status */
	function isort(a, status)
	{
		var i;
		var aNew = new Array();

		if(status)
		{
			app.thermometer.begin();
			app.thermometer.duration = a.length;
			app.thermometer.text = status;
		}
		for(i = 0; i < a.length; i++)
		{
			if(status)
				app.thermometer.value = i;
			binsert(aNew, a[i]);
		}
		if(status)
			app.thermometer.end();
		return aNew;
	}

	function ANsummarize(doc, title, p, r, dest, fs)
	{	/* Summarize annotations sorted primarily by property p */
		app.thermometer.begin();
		app.thermometer.text = IDS_PROGRESS_SUMMARIZE;

		if(!ANsums[p])
			p = ANSB_Page;
		if(!title)
			title = IDS_UNNAMED;

		/* make sure we have all annots */
		this.syncAnnotScan();

		/* Get all summarizable annots on all pages sorted in the given manner */
		var a = doc.getAnnots(-1, p, r, ANFB_ShouldSummarize);
		var t, s;
		var r = new Report();

		r.style = "NoteTitle";
		r.size = 3;
		t = IDS_SUM_TITLE1 + title + IDS_SUM_TITLE2;
		r.writeText(t);
		r.divide(3.5);

		var i, j, contents;
		var oldHeading;

		if(a && a.length > 0)
		{
		  app.thermometer.duration = a.length;
		  for(i = 0; i < a.length; i++)
		  {
			app.thermometer.value = i;
			// maybe do the heading
			  r.style = "NoteTitle";
			  r.size = 2;
			  var heading = (ANsums[p])(a[i]);
              if(heading != oldHeading)
			  {
                if(typeof oldHeading != "undefined")
				  r.writeText(" ");
			    r.writeText(heading);
				oldHeading = heading;
				r.divide();
			  }

			  for(j = 0; j < ANsumorder.length; j++)
				  if(ANsumorder[j] != p)
				  {
					  r.size = 1;
					  r.writeText((ANsums[ANsumorder[j]])(a[i]));
				  }
			  var contents = a[i].contents;
			  if(contents)
			  {
				  r.style = "DefaultNoteText";
				  r.size = 1;
				  r.indent();
				  r.writeText(contents);
				  r.writeText(" ");
				  r.outdent();
			  }
			  else
				  r.writeText(" ");
		  }
		}
		else
		  r.writeText(IDS_SUM_NO_ANNOTS1 + title + IDS_SUM_NO_ANNOTS2);
		if (typeof dest != "undefined")
			r.save(dest, fs);
		else
			r.open(t);
		app.thermometer.end();

		return a ? a.length : 0;
	}

	/* flags used by collaboration
	*/
	CBFNiceTableName = 1;
	CBFNiceDBName = 2;
	CBFDBPerDoc = 4;

	function CBgetTableDesc(doc, author)
	{
	  var frag = Collab.URL2PathFragment(doc.URL);
	  var DBName;
	  var tableName;

	  if(doc.collabDBFlags & CBFDBPerDoc)
	  {
		DBName = frag;
		tableName = author;
	  }
	  else
	  {
		DBName = "";
		tableName = frag;
	  }

	  if(doc.collabDBFlags & CBFNiceTableName)
		tableName = Collab.hashString(tableName);
	  if(doc.collabDBFlags & CBFNiceDBName)
		DBName = Collab.hashString(DBName);
	  return {DBName: doc.collabDBRoot + DBName,
		tableName: tableName,
		URL: doc.URL,
		user: author,
		flags: doc.collabDBFlags};
	}

	function CBgetTableConnect(desc)
	{
	  var e;

	  try
	  {
		var conn = ADBC.newConnection(desc.DBName);
		var stmt = conn.newStatement();

		return {conn: conn,
		  stmt: stmt,
		  tableName: desc.tableName,
		  user: desc.user,
		  flags: desc.flags};
	  }
	  catch(e) { debugExcept(e); return false; }
	}

	function CBgetInfo(conn, name)
	{
	  var e;

	  try
	  {
		conn.stmt.execute("select CONTENTS from \"" + conn.tableName + "\" where AUTHOR like ?;",
		  "~" + name + "~");
		conn.stmt.nextRow();
		return conn.stmt.getColumn("CONTENTS").value;
	  }
	  catch(e) { debugExcept(e); return false; }
	}

	function CBsetInfo(conn, name, value)
	{
	  var e;

	  /* add the field */
	  try { return conn.stmt.execute("insert into \"" + conn.tableName + "\" (AUTHOR, CONTENTS) values (?, ?);",
		  "~" + name + "~",
		  value); }
	  catch(e) { debugExcept(e); return false; }
	}

	function CBcreateTable(desc)
	{
	  var e;

	  try
	  {
		var conn = ADBC.newConnection(desc.DBName);
		var stmt = conn ? conn.newStatement() : null;

		/* come up with the SQL query to do it */
		var sql1 = "create table \"" + desc.tableName + "\" (AUTHOR varchar(64), PAGE integer, NAME varchar(64), CONTENTS text, DATA image);";
		var sql2 = "create table \"" + desc.tableName + "\" (AUTHOR varchar(64), PAGE integer, NAME varchar(64), CONTENTS clob, DATA blob);";

		var conn = {conn: conn,
		  stmt: stmt,
		  tableName: desc.tableName,
		  user: desc.user,
		  flags: desc.flags};

		// first try...
		try
		{
		  stmt.execute(sql1);
		} catch(e) { debugExcept(e); }
		// second try...
		try
		{
		  stmt.execute(sql2);
		} catch(e) { debugExcept(e); }
		// these will throw if the table wasn't created
		CBsetInfo(conn, "URL", desc.URL);
		CBsetInfo(conn, "creator", desc.user);
		return conn;
	  }
	  /* we failed... */
	  catch(e) { debugExcept(e); return false; }
	}

	function CBconnect(desc, bDoNotCreate)
	{
	  var conn = CBgetTableConnect(desc);
	  var e;

	  /* if we can't get the URL from it, it doesn't exist */
	  if(!CBgetInfo(conn, "URL"))
	  {
		if (!bDoNotCreate)
		  conn = CBcreateTable(desc);
		else
		  return false;
	  }

	  /* here it is! */
	  return conn;
	}

	/* mapping of annot types to data properties */
	CBannotdata =
	{
		FileAttachment:	"FSCosObj",
		Sound:			"SCosObj"
	};

	/* returns the data fork for an annot */
	function CBannotData(annot)
	{
	  var prop = CBannotdata[annot.type];
	  var stm = prop ? Collab.cosObj2Stream(annot[prop]) : null;

	  if(stm)
		stm.type = ADBC.SQLT_LONGVARBINARY;
	  return stm;
	}

	/* sets the data fork of an annot */
	function CBannotSetData(annot, data)
	{
	  var prop = CBannotdata[annot.type];

	  if(prop)
		annot[prop] = data;
	}


	/* recursive function that deletes a reply chain */
	function CBDeleteReplyChain(disc)
	{
		var replies = Discussions.getDiscussions(disc);

		if (replies && (replies.length == 1))
		{
			var currentReply = replies[0];
			var looper = 1;
			while (looper)
			{
				/*
				** There better only be one reply 
				*/
				var saveChild = Discussions.getDiscussions(currentReply);

	//			console.println("Delete reply");
				currentReply.Delete();

				if (saveChild && (saveChild.length == 1))
					currentReply = saveChild[0];
				else
					looper = 0;
			}
		}

	}

	/* gets the reply chain, stuffs it in a stream */
	/* and then puts it in the annot */
	function CBGetReplyChain(dstAnnot, discussion)
	{
		var discList = Discussions.getDiscussions(discussion);

		var cos = Collab.newWrStreamToCosObj();

		var data = 0;
		while (discList && (discList.length > 0))
		{
			data = 1;
			cos.write(discList[0].Text);
	//		console.println("Write to cos stream " + discList[0].Text.length + " characters");

			discList = Discussions.getDiscussions(discList[0]);
		}

		if (data == 1)
			CBannotSetData(dstAnnot, cos.getCosObj());
	}

	/* get the stream and puts the data as replies */
	function CBPutReplyChain(discussion, bookmark, srcAnnot)
	{
		var cosStream = CBannotData(srcAnnot);

		if(cosStream)
		{
			var s = cosStream.read(Collab.wdBlockSize);

			while (discussion && (s.length > 0))
			{
				discussion = Discussions.addDiscussion(discussion, "Data", s, bookmark);

				s = null;
			
				s = cosStream.read(Collab.wdBlockSize);
			}
		}
	}

	/* ADBC based annot enumerator constructor
	*/
	function ADBCAnnotEnumerator(parent, sorted)
	{
	  /* store away parameters */
	  this.parent = parent;
	  this.sorted = sorted;
	  /* add enumeration method */
	  this.next = function()
	  {
		var e;

		try
		{
		  if(!this.conn)
		  {
			this.conn = CBconnect(this.parent.desc, true);
			this.conn.stmt.execute("select CONTENTS from \"" + this.parent.desc.tableName + "\" where AUTHOR not like '~%~'" +
			  (this.sorted ? " order by PAGE, NAME;" : ";"));
		  }
		  this.conn.stmt.nextRow();
		  return eval(this.conn.stmt.getColumn("CONTENTS").value);
		}
		catch(e) { debugExcept(e); return false; }
	  }
	}

	/* ADBC based annot store constructor
	*/
	function ADBCAnnotStore(doc, user)
	{
	  this.desc = CBgetTableDesc(doc, user);
	  this.enumerate = function(sorted)
	  {
		return new ADBCAnnotEnumerator(this, sorted);
	  }
	  this.complete = function(toComplete)
	  {
		var i;
		var conn = CBconnect(this.desc,true);

		if (conn) 
			{
		  for(i = 0; toComplete && i < toComplete.length; i++)
		  {
			if(CBannotdata[toComplete[i].type])
			{
			  var e;
  
			  try
			  {
				conn.stmt.execute("select DATA from \"" + this.desc.tableName + "\" where PAGE = ? and NAME like ?;",
				  toComplete[i].page, toComplete[i].name);
				conn.stmt.nextRow();
				var cos = Collab.newWrStreamToCosObj();

				conn.stmt.getColumn("DATA", ADBC.Binary | ADBC.Stream, cos);
				CBannotSetData(toComplete[i], cos.getCosObj());
			  }
			  catch(e) { debugExcept(e);}
			}
			  }
		}
		return true;
	  }
	  this.update = function(toDelete, toAdd, toUpdate)
	  {
		var i;
		var e;
		var conn = CBconnect(this.desc);

		for(i = 0; toDelete && i < toDelete.length; i++)
		{
		  try
		  {
			conn.stmt.execute("delete from \"" + this.desc.tableName + "\" where PAGE = ? and NAME like ?;",
			  toDelete[i].page, toDelete[i].name);
		  }
		  catch(e) { debugExcept(e);}
		}
		for(i = 0; toAdd && i < toAdd.length; i++)
		{
		  try
		  {
			conn.stmt.execute("insert into \"" + this.desc.tableName + "\" (AUTHOR, PAGE, NAME, CONTENTS, DATA) values (?, ?, ?, ?, ?);",
			  toAdd[i].author, toAdd[i].page, toAdd[i].name, toAdd[i].toSource(), CBannotData(toAdd[i]));
		  }
		  catch(e) { debugExcept(e);}
		}
		for(i = 0; toUpdate&& i < toUpdate.length; i++)
		{
		  try
		  {
			conn.stmt.execute("update \"" + this.desc.tableName + "\" set CONTENTS = ?, DATA = ? where PAGE = ? and NAME like ?;",
			  toUpdate[i].toSource(), CBannotData(toUpdate[i]), toUpdate[i].page, toUpdate[i].name);
		  }
		  catch(e) { debugExcept(e);}
		}
		return true;
	  }
	}

	/* Munge an URL such that Web Discussions won't put our data in the discussions pane
	*/
	function WDmungeURL(url)
	{
		return url + "/ACData";
	}

	/* Web discussions based annot enumerator constructor
	*/
	function WDAnnotEnumerator(parent, sorted)
	{
	//  console.println("WDAnnotEnumerator(): Begin");

	  this.parent = parent;
	  this.sorted = sorted;
	  this.next = function()
	  {
	//	console.println("WDAnnotEnumerator.next(): Begin");

		if(!this.discussions)
		{
	//		console.println("WDAnnotEnumerator.next(): get discussions "+WDmungeURL(this.parent.doc.URL));

		  this.discussions = Discussions.getDiscussions(WDmungeURL(this.parent.doc.URL));

		  app.thermometer.begin();
		  app.thermometer.text = IDS_PROGRESS_FETCHING;
		  if(this.discussions) // always sort as our completion callback relies on a sorted list
		  {
			this.discussions = isort(this.discussions, IDS_PROGRESS_SORTING);
			app.thermometer.duration = this.discussions.length;
		  }
		  this.index = 0;
		}
		/* skip non-Acro discussions */
		while(this.discussions && this.index < this.discussions.length && this.discussions[this.index] == "[Discussion]")
		  app.thermometer.value = this.index++;
		if(!this.discussions || this.index >= this.discussions.length)
		{
		  app.thermometer.end();
		  return false;
		}
		return eval(this.discussions[this.index++].Text);
	  }
	}

	/* Web discussion based annot store constructor
	*/
	function WDAnnotStore(doc, user)
	{
	//  console.println("WDAnnotStore(): Begin");

	  this.doc = doc;
	  this.user = user;
	  this.enumerate = function(sorted)
	  {
	//	console.println("WDAnnotStore.enumerate(): Begin");
		return new WDAnnotEnumerator(this, sorted);
	  }
	  this.complete = function(toComplete)
	  {
	//	console.show();
	//	console.println("WDAnnotStore.toComplete(): Begin");

		var i,j;
	//	console.println("get discussions for "+WDmungeURL(this.doc.URL));
		var discussions = Discussions.getDiscussions(WDmungeURL(this.doc.URL));

		if (discussions && discussions.length) 
		{
			// sort them to perform fast searches
			// JS sort is a SLOW qsort... use our worst case N log ( N )
			discussions = isort(discussions, IDS_PROGRESS_SORTING);
 
			app.thermometer.begin();
			app.thermometer.text = IDS_PROGRESS_FETCHING_BIG;
			app.thermometer.duration = toComplete.length;
			for(i = 0, j = 0; discussions && (i < toComplete.length) && (j < discussions.length); app.thermometer.value = ++i)
			{
				//console.println("disussion " + i);

				// create a string that'll look like the corresponding discussion
				var discString = Discussions.makeDiscussionString(toComplete[i].page, toComplete[i].name);
				//console.println("Descriptive string \"" + discString + "\"");

				// keep skipping annots while they are "less" than the current one
				while(discString > discussions[j])
					j++;

				// if we found it
				if(discString == discussions[j])
				{
					//console.println("found it - Annot to Complete " + i + " is in discussion slot " + j);
					//console.println("subject "+discussions[j].Subject);

					/*
					** We found the discussion, now gather replys which will
					** contain the "data" for the stream
					*/
					if (CBannotdata[toComplete[i].type])
						CBGetReplyChain(toComplete[i], discussions[j]);

				}

			}
			app.thermometer.end();
		}
		return true;
	  }
	  this.update = function(toDelete, toAdd, toUpdate)
	  {
	//	console.println("WDAnnotStore.update(): Begin");

		// get the list of discussions
	//	console.println("WDAnnotStore.update(): get discussions "+WDmungeURL(this.doc.URL();
		var discussions = Discussions.getDiscussions(WDmungeURL(this.doc.URL));
		var i, j;

		// if we got any...
		if(discussions && discussions.length)
		{
	//		console.println("WDAnnotStore.update(): got some " + discussions.length);
			// sort them to perform fast searches
			discussions = isort(discussions, IDS_PROGRESS_SORTING);

			// if we've got any to update
			if(toUpdate && toUpdate.length)
			{
				app.thermometer.begin();
				app.thermometer.text = IDS_PROGRESS_CHANGING;
				app.thermometer.duration = toUpdate.length;
	//			console.println("WDAnnotStore.update(): updating " + toUpdate.length);

				for(i = 0, j = 0; i < toUpdate.length && j < discussions.length; app.thermometer.value = ++i)
				{
				  // create a string that'll look like the corresponding discussion
				  var discString = Discussions.makeDiscussionString(toUpdate[i].page, toUpdate[i].name);

				  // keep skipping annots while they are "less" than the current one
				  while(discString > discussions[j])
					j++;
				  // if we found it
				  if(discString == discussions[j])
				  {
					// then update it!
					CBDeleteReplyChain(discussions[j]);
					discussions[j].Delete();

					var bookmark = Discussions.makeBookmark(toUpdate[i].page, toUpdate[i].name);
					discussions[j] = Discussions.addDiscussion(WDmungeURL(this.doc.URL), "Markup", toUpdate[i].toSource(), bookmark);
					CBPutReplyChain(discussions[j], bookmark, toUpdate[i]);
					j++;
				  }
				}
				app.thermometer.end();
			}

			// delete is just like update
			if(toDelete && toDelete.length) 
			{
				app.thermometer.begin();
				app.thermometer.text = IDS_PROGRESS_DELETING;
				app.thermometer.duration = toDelete.length;
	//			console.println("WDAnnotStore.update(): deleting " + toDelete.length);
				for(i = 0, j = 0; i < toDelete.length && j < discussions.length; app.thermometer.value = ++i)
				{
				  var discString = Discussions.makeDiscussionString(toDelete[i].page, toDelete[i].name);

				  while(discString > discussions[j])
					j++;

				  if(discString == discussions[j])
				  {
					CBDeleteReplyChain(discussions[j]);

					discussions[j].Delete();

					j++;
				  }
				}
				app.thermometer.end();
			}
		}
		if(toAdd && toAdd.length)
		{
			app.thermometer.begin();
			app.thermometer.text = IDS_PROGRESS_ADDING;
			app.thermometer.duration = toAdd.length;
			for(i = 0; toAdd && i < toAdd.length; app.thermometer.value = ++i)
			{
			  var bookmark = Discussions.makeBookmark(toAdd[i].page, toAdd[i].name);

		/*
			  console.println("WDAnnotStore.update(): adding " + toAdd.length);
			  console.println("this.doc.URL \""+ WDmungeURL(this.doc.URL) + "\"");
			  console.println("Markup");
			  console.println("toAdd[i].toSource() \""+ toAdd[i].toSource() + "\"");
			  console.println("bookmark \"" + bookmark + "\"");
		*/

			  var discussion = Discussions.addDiscussion(WDmungeURL(this.doc.URL), "Markup", toAdd[i].toSource(), bookmark);

			  if (discussion && CBannotdata[toAdd[i].type])
				CBPutReplyChain(discussion, bookmark, toAdd[i]);

			}
			app.thermometer.end();
		}
		return true;
	  }
	}

	/* Set up default annot stores */
	Collab.addAnnotStore("NONE", IDS_STORE_NONE,
		{create: function(doc, user, settings){ return null; }});
	Collab.setStoreNoSettings("NONE", true);
	if(typeof Discussions != "undefined")
	{
	  Collab.addAnnotStore("WD", IDS_STORE_WEB_DISCUSSIONS,
			{create: function(doc, user, settings){ return new WDAnnotStore(doc, user); }});
		Collab.setStoreNoSettings("WD", true);
	}
	if(typeof ADBC != "undefined")
		Collab.addAnnotStore("DB", IDS_STORE_DATABASE,
			{create: function(doc, user, settings){ doc.collabDBRoot = settings; doc.collabDBFlags = CBFNiceTableName; return (settings && settings != "") ? new ADBCAnnotStore(doc, user) : null; }});
	Collab.addAnnotStore("DAVFDF", IDS_STORE_DAVFDF,
		{create: function(doc, user, settings){ return (settings && settings != "") ? new FSAnnotStore(doc, user, settings + doc.Collab.docID + "/", "CHTTP") : null; }});
	Collab.addAnnotStore("FSFDF", IDS_STORE_FSFDF,
		{create: function(doc, user, settings){ return (settings && settings != "") ? new FSAnnotStore(doc, user, settings + doc.Collab.docID + "/") : null; }});
	Collab.setStoreFSBased("FSFDF", true);

	// Web Discussion data block size
	Collab.wdBlockSize = 16384;
}

function CBdef(a, b)
{
  return typeof a == "undefined" ? b : a;
}

function Matrix2D(a, b, c, d, h, v)
{
	this.a = CBdef(a, 1);
	this.b = CBdef(b, 0);
	this.c = CBdef(c, 0);
	this.d = CBdef(d, 1);
	this.h = CBdef(h, 0);
	this.v = CBdef(v, 0);
	this.fromRotated = function(doc, page)
	{
		page = CBdef(page, 0);

		var cropBox = doc.getPageBox("Crop", page);
		var mediaBox = doc.getPageBox("Media", page);
		var mbHeight = mediaBox[1] - mediaBox[3];
		var mbWidth = mediaBox[2] - mediaBox[0];
		var rotation = doc.getPageRotation(page);
		var m = new Matrix2D(1, 0, 0, 1, cropBox[0] - mediaBox[0], cropBox[3] - mediaBox[3]);

		if(rotation == 90)
			return this.concat(m.rotate(Math.asin(1.0)).translate(mbHeight, 0));
		else if(rotation == 180)
			return this.concat(m.rotate(2.0 * -Math.asin(1.0)).translate(mbWidth, mbHeight));
		else if(rotation == 270)
			return this.concat(m.rotate(-Math.asin(1.0)).translate(0, mbWidth));
		return this.concat(m);
	}
	this.transform = function(pts)
	{
		var result = new Array(pts.length);

		if(typeof pts[0] == "object")
			for(var n = 0; n < pts.length; n++)
				result[n] = this.transform(pts[n]);
		else
			for(var n = 0; n + 1 < pts.length; n += 2)
			{
				result[n] = this.a * pts[n] + this.c * pts[n + 1] + this.h;
				result[n + 1] = this.b * pts[n] + this.d * pts[n + 1] + this.v;
			}
		return result;
	}
	this.concat = function(m)
	{
		return new Matrix2D(
			(this.a * m.a) + (this.b * m.c),
			(this.a * m.b) + (this.b * m.d),
			(this.c * m.a) + (this.d * m.c),
			(this.c * m.b) + (this.d * m.d),
			(this.h * m.a) + (this.v * m.c) + m.h,
			(this.h * m.b) + (this.v * m.d) + m.v);
	}
	this.invert = function()
	{
		var result = new Matrix2D;
		var q = this.b * this.c - this.a * this.d;

		if (q)
		{
			result.a = - this.d / q;
			result.b = this.b / q;
			result.c = this.c / q;
			result.d = - this.a / q;
			result.h = -(this.h * result.a + this.v * result.c);
			result.v = -(this.h * result.b + this.v * result.d);
		}
		return result;
	}
	this.translate = function(dx, dy)
	{
		return this.concat(new Matrix2D(1, 0, 0, 1, CBdef(dx, 0), CBdef(dy, 0)));
	}
	this.scale = function(sx, sy)
	{
		return this.concat(new Matrix2D(CBdef(sx, 1), 0, 0, CBdef(sy, 1), 0, 0));
	}
	this.rotate = function(t)
	{
		t = CBdef(t, 0);
		return this.concat(new Matrix2D(Math.cos(t), Math.sin(t), -Math.sin(t), Math.cos(t), 0, 0));
	}
}
