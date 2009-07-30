<?xml version='1.0'?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		<html>
			<font face="sans-serif">
				<h2>Automatic KBase2HTML </h2>
				<h5>
					<a href="http://www.confusionists.com">Confusionists.Inc.</a>
				</h5>
				<h3>Snippets View 1</h3>
				<ul>
					<xsl:apply-templates select="Kbase/TopLevelIds"/>
				</ul>
				<h3>Snippets View 2</h3>
				<xsl:apply-templates select="Kbase/Snippets"/>
			</font>
		</html>
	</xsl:template>

	<!-- top level child ids -->
	<xsl:template match="Child">
		<xsl:call-template name="SnippetView1">
			<xsl:with-param name="idToFind" select="text()"/>
			<xsl:with-param name="growingId" select="text()"/>
		</xsl:call-template>
	</xsl:template>

	<!-- recursive -->
	<xsl:template name="SnippetView1">
		<xsl:param name="idToFind"/>
		<xsl:param name="growingId"/>
		<a>
			<xsl:attribute name="NAME">
				Instance_<xsl:value-of select="$growingId"/>
			</xsl:attribute>
		</a>

		<xsl:call-template name="View1SnippetRef">
			<xsl:with-param name="idToFind" select="$idToFind"/>
		</xsl:call-template>
		<ul>
			<xsl:for-each select="//Snippet[Id=$idToFind]/Child">
				<xsl:call-template name="SnippetView1">
					<xsl:with-param name="growingId" select="concat(concat($growingId,'.'),text())"/>
					<xsl:with-param name="idToFind" select="text()"/>
				</xsl:call-template>
			</xsl:for-each>
		</ul>
	</xsl:template>

	<xsl:template name="View1SnippetRef">
		<xsl:param name="idToFind"/>
		<li>
			<a>
				<xsl:attribute name="href">
					#Snippet_<xsl:value-of select="$idToFind"/>
				</xsl:attribute>
				<xsl:value-of select="//Snippet[Id=$idToFind]/Title"/>
			</a>
		</li>
	</xsl:template>


	<xsl:template match="Snippet">
		<h3>
			<a>
				<xsl:attribute name="NAME">
					Snippet_<xsl:value-of select="Id"/>
				</xsl:attribute>
				<xsl:value-of select="Title"/>
			</a>
		</h3>

		<xsl:variable name="id" select="Id"/>
		<xsl:choose>
			<xsl:when test="count(//Snippet[Child=$id])>0">
				Parents
			</xsl:when>
			<xsl:otherwise>
				Top Level Snippet
			</xsl:otherwise>
		</xsl:choose>
		<xsl:for-each select="//Snippet[Child=$id]">
			<xsl:call-template name="ParentReffff">
				<xsl:with-param name="idToFind" select="Id"/>
				<xsl:with-param  name="growingId" select="Id"/>
			</xsl:call-template>
		</xsl:for-each>



		<xsl:if test="count(Child)>0">
			<br/>
			<br/>
			Children
			<li>
				<xsl:for-each select="Child">
					<xsl:call-template name="View2ChildRef">
						<xsl:with-param name="idToFind" select="."/>
					</xsl:call-template><xsl:if test="position() &lt; count(../Child)">,</xsl:if>&#160;
				</xsl:for-each>
			</li>
		</xsl:if>

		<br/>
		<br/>

		<textarea style="overflow:visible" readonly="readonly" cols="80">
			<xsl:value-of select="Text"/>
		</textarea>
		<br/>
		<hr/>


	</xsl:template>


	<xsl:template name="ParentReffff">
		<xsl:param name="idToFind"/>
		<xsl:param name="growingId"/>
		<xsl:for-each select="//Snippet[Child=$idToFind]">
			<xsl:call-template name="ParentReffff">
				<xsl:with-param name="idToFind" select="Id"/>
				<xsl:with-param name="growingId" select="concat(concat(Id,'.'), $growingId)"/>
			</xsl:call-template>
		</xsl:for-each>

		<xsl:choose>
			<xsl:when test="count(//Snippet[Child=$idToFind])=0">
				<br/>
				<font size="-3">
					<a>
						<xsl:attribute name="href">
							#Instance_<xsl:value-of select="$growingId"/>
						</xsl:attribute>[in tree]
					</a>
				</font>&#160;
			</xsl:when>

			<xsl:otherwise>
				-->
			</xsl:otherwise>
		</xsl:choose>

		<a>
			<xsl:attribute name="href">
				#Snippet_<xsl:value-of select="$idToFind"/>
			</xsl:attribute>
			<xsl:value-of select="//Snippet[Id=$idToFind]/Title"/>
		</a>

	</xsl:template>


	<xsl:template name="View2ChildRef">
		<xsl:param name="idToFind"/>
		<a>
			<xsl:attribute name="href">
				#Snippet_<xsl:value-of select="$idToFind"/>
			</xsl:attribute>
			<xsl:value-of select="//Snippet[Id=$idToFind]/Title"/>
		</a>
	</xsl:template>

</xsl:stylesheet>
<!-- Stylus Studio meta-information - (c)1998-2004. Sonic Software Corporation. All rights reserved.
<metaInformation>
<scenarios ><scenario default="yes" name="Kbase1" userelativepaths="yes" externalpreview="yes" url="sample.kbase.xml" htmlbaseurl="" outputurl="output.html" processortype="internal" profilemode="0" profiledepth="" profilelength="" urlprofilexml="" commandline="" additionalpath="" additionalclasspath="" postprocessortype="none" postprocesscommandline="" postprocessadditionalpath="" postprocessgeneratedext=""/></scenarios><MapperMetaTag><MapperInfo srcSchemaPathIsRelative="yes" srcSchemaInterpretAsXML="no" destSchemaPath="" destSchemaRoot="" destSchemaPathIsRelative="yes" destSchemaInterpretAsXML="no"/><MapperBlockPosition></MapperBlockPosition></MapperMetaTag>
</metaInformation>
-->
<!-- Stylus Studio meta-information - (c)1998-2004. Sonic Software Corporation. All rights reserved.
<metaInformation>
<scenarios/><MapperMetaTag><MapperInfo srcSchemaPathIsRelative="yes" srcSchemaInterpretAsXML="no" destSchemaPath="" destSchemaRoot="" destSchemaPathIsRelative="yes" destSchemaInterpretAsXML="no"/><MapperBlockPosition></MapperBlockPosition></MapperMetaTag>
</metaInformation>
-->