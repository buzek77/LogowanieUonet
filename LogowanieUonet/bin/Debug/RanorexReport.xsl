<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="message">

    <!--<xsl:choose>
          <xsl:when test ="./Result/State = $TRUE">
          <xsl:variable name="Path" select="./General/TestcaseName"/>
            <td width="32%" bgcolor="#00FF00"><a href="{./General/TestcaseName/@href}"><xsl:value-of select ="./General/TestcaseName"/></a></td>
            <td width="68%" bgcolor="#00FF00"><xsl:value-of select ="./Result/State"/></td>
          </xsl:when>
          <xsl:otherwise >
            <td width="32%" bgcolor="#FF0000"><a href="{./General/TestcaseName/@href}"><xsl:value-of select ="./General/TestcaseName"/></a></td>
            <td width="68%" bgcolor="#FF0000"><xsl:value-of select ="./Result/State"/></td>
          </xsl:otherwise>
        </xsl:choose>
		-->
    <xsl:choose>
      <xsl:when test ="@level = 'WARN'">
        <tr class="warn {./@category}Warn" style="{@style}">
		  <td>
            <xsl:value-of select ="./@time"/>
          </td>
          <td>
            <xsl:value-of select ="./@level"/>
          </td>
          <td>
            <xsl:value-of select ="./@category"/>
          </td>
          <td>
           <xsl:copy-of select ="child::node()"/>
          </td>
        </tr>
      </xsl:when>
      <xsl:when test ="@level = 'ERROR'">
        <tr class="error {./@category}Error" style="{@style}">
		  <td>
            <xsl:value-of select ="./@time"/>
          </td>
          <td>
            <xsl:value-of select ="./@level"/>
          </td>
          <td>
            <xsl:value-of select ="./@category"/>
          </td>
          <td>
           <xsl:copy-of select ="child::node()"/>
          </td>
        </tr>
      </xsl:when>
      <xsl:when test ="@level = 'DEBUG'">
        <tr class="debug" style="{@style}">
          <td>
            <xsl:value-of select ="./@time"/>
          </td>
          <td>
            <xsl:value-of select ="./@level"/>
          </td>
          <td>
            <xsl:value-of select ="./@category"/>
          </td>
          <td>
           <xsl:copy-of select ="child::node()"/>
          </td>
        </tr>
      </xsl:when>
      <xsl:when test ="@level = 'SUCCESS'">
        <tr class="success" style="{@style}">
          <td>
            <xsl:value-of select ="./@time"/>
          </td>
          <td>
            <xsl:value-of select ="./@level"/>
          </td>
          <td>
            <xsl:value-of select ="./@category"/>
          </td>
          <td>
           <xsl:copy-of select ="child::node()"/>
          </td>
        </tr>
      </xsl:when
		>
      <xsl:when test ="@level = 'FAILURE'">
        <tr class="failure" style="{@style}">
          <td>
            <xsl:value-of select ="./@time"/>
          </td>
          <td>
            <xsl:value-of select ="./@level"/>
          </td>
          <td>
            <xsl:value-of select ="./@category"/>
          </td>
          <td>
           <xsl:copy-of select ="child::node()"/>
          </td>
        </tr>
      </xsl:when>
      <xsl:otherwise>
        <tr class="info {./@category}Info" style="{@style}">
          <td>
            <xsl:value-of select ="./@time"/>
          </td>
          <td>
            <xsl:value-of select ="./@level"/>
          </td>
          <td>
            <xsl:value-of select ="./@category"/>
          </td>
          <td>
           <xsl:copy-of select ="child::node()"/>
          </td>
        </tr>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template match="/log">
    <html>
      <head>
        <title>
          <xsl:value-of select ="/log/@title"/>
        </title>
        <link rel="stylesheet" type="text/css" href="http://www.ranorex.com/fileadmin/templates/NewDesign/css/style.css" />
        <style type="text/css">

          html,body
          {
          color: black; 	background-color: #ebebeb;

          font-size: 1em;
          font-family: Verdana;

          margin: 0; padding: 0;
          text-align: center;
          }

          body, html, div, p, i, strong, table {color:#606060; }
          img{border:0px;}
          div#Content img { margin: 7px 0;}

          p,i
          {
          margin: 0;
          padding: 3px 0px;
          }

          div#Content table
          {
          font-size: 1em;
          margin: 2px;
          }

          table td
          {
          padding: 3px 4px;
          margin: 0;
          }


          a:link
          {
          color: #c11111;
          }
          a:visited
          {
          color: #db4848;

          }
          a:active
          {
          color: #c11111;

          }
          a:hover
          {
          color: black;

          }


          div#page
          {
          text-align: left;
          margin: 0 auto ;
          width: 850px;
          padding:  20px 0 0 0 ;
          }


          div#Content
          {
          float: none;
          font-size: 0.7em;
          background-color: #fff;
          margin: 0 35px 30px 35px;
          padding: 0;
          width: auto;
          }
          * html div#Content
          {
          height: 1em;
          margin:  0 35px 30px 35px;
          padding:0;
          }

          h1
          {
          font-size: 13pt;
          }

          th
          {
          background-color: #eee;
          text-align: left;
          padding: 10px 4px;
          }

          td
          {
          border-bottom: 1px solid #eee;
          padding: 0;
          }

          .debug
          {
          color: gray;
          }

          .invisible
          {
          display:none;
          }

          .warn
          {
          color: #EFB54A;
          font-weight: bold;
          }

          .error
          {
          color: red;
          font-weight: bold;
          }


          .failure
          {
          color: #fff;
          background-color: #d80000;
          font-weight: bold;
          }

          .success
          {
          color: #ffffff;
          background-color: green;
          font-weight: bold;
          }



          fieldset
          {
          clear: both;
          font-size: 100%;
          border-color: #000000;
          border-width: 0;
          border-style: solid none none none;
          padding: 5px;
          margin: 3px 2px 10px 2px;
          }

          fieldset legend
          {
          font-size:	10pt;
          color: #555;
          font-weight: bold;
          margin: 0 0 0 -7px;
          padding: 10px 0 0 0;
          }
          label
          {
          float: left;
          text-align: right;
          width: auto;
          margin-right: 0em;
          padding: 2px 0;
          }
          input
          {
          float: left;
          margin-right: 30px;
          }

        </style>
        <script type="text/javascript">
          <![CDATA[ 
			function switchVisibility(firstclassName, excludeClassname)
			{
				var tr = document.getElementsByTagName('tr');
				for(var i=0;i<tr.length;i++)
				{
					if(tr[i].className.search(firstclassName + ' invisible')>=0 && (tr[i].className.search(excludeClassname)<0 || excludeClassname=="")) 
						tr[i].className = tr[i].className.replace(firstclassName + ' invisible', firstclassName);
					else if(tr[i].className.search(firstclassName)>=0 && (tr[i].className.search(excludeClassname)<0 || excludeClassname==""))
					  	tr[i].className = tr[i].className.replace(firstclassName, firstclassName + ' invisible');
				}
			}
			 ]]>
        </script>

      </head>
      <body>
        <div id="page">
          <div id="header-top">
            <h1 id="logo">
              <a href="#" title="Ranorex">
                <em>Ranorex</em>
              </a>
            </h1>
            <div id="small-nav"></div>
          </div>

          <div id="container">
            <div id="Content">
              <h1>
                <xsl:value-of select ="/log/@title"/>
              </h1>
              <fieldset>
                <legend>Log Level</legend>
                <label for="debug">Debug</label>
                <input type="checkbox" id="debug" name="debug" onClick="switchVisibility('debug','Validation');" checked="1" />
                <label for="info">Info</label>
                <input type="checkbox" id="info" name="info" onClick="switchVisibility('info','Validation');" checked="1" />
                <label for="warn">Warn</label>
                <input type="checkbox" id="warn" name="warn" onClick="switchVisibility('warn','Validation');" checked="1" />
                <label for="error">Error</label>
                <input type="checkbox" id="error" name="error" onClick="switchVisibility('error','Validation');" checked="1" />

                <label for="testresult">Test Result</label>
                <input type="checkbox" id="testresult" name="testresult" onClick="switchVisibility('failure','success');switchVisibility('success','failure')" checked="1" />

              </fieldset>
              <table border="0" cellspacing="0" width="100%">
                <tr>
                  <th width="15%" >
                    <b>Time</b>
                  </th>
                  <th width="10%" >
                    <b>Level</b>
                  </th>
                  <th width="15%" >
                    <b>Category</b>
                  </th>
                  <th>
                    <b>Message</b>
                  </th>
                </tr>
                <xsl:apply-templates select="//message"/>
              </table>

            </div>
            <div style="clear:both"></div>
            <div id="Footer"></div>
          </div>
        </div>
      </body>
    </html>
  </xsl:template>

  <!--
  Template zum Einfuegen von Linefeeds:
  Alle in der Description angegebenen "<lf/>" TAGS werden in "<br/>" TAGS umgewandelt.
  -->
  <xsl:template match="lf">
    <br/>
  </xsl:template>

</xsl:stylesheet>

