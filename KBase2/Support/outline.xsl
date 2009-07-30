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
				<ol>
					<xsl:apply-templates select="Kbase/TopLevelIds"/>
				</ol>
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

		<xsl:call-template name="View1SnippetRef">
			<xsl:with-param name="idToFind" select="$idToFind"/>
		</xsl:call-template>
		<ol>
			<xsl:for-each select="//Snippet[Id=$idToFind]/Child">
				<xsl:call-template name="SnippetView1">
					<xsl:with-param name="growingId" select="concat(concat($growingId,'.'),text())"/>
					<xsl:with-param name="idToFind" select="text()"/>
				</xsl:call-template>
			</xsl:for-each>
		</ol>
	</xsl:template>

	<xsl:template name="View1SnippetRef">
		<xsl:param name="idToFind"/>
		<li>
				<xsl:value-of select="//Snippet[Id=$idToFind]/Title"/>
		</li>
	</xsl:template>




</xsl:stylesheet>
