<?xml version='1.0' encoding='utf-8'?>
<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
    xmlns:msxsl='urn:schemas-microsoft-com:xslt' exclude-result-prefixes='msxsl'
xmlns:cce='comdiv://config/extensions'
>
    <xsl:output method='xml' indent='yes'/>
    <xsl:variable name='options' select='document(".my")//cce:option' />
  <xsl:variable name='thisFile' select='document("")' />

    <xsl:template match='@* | node()'>
        <xsl:copy>
            <xsl:apply-templates select='@* | node()'/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match='/config'>
        <xsl:copy>
        <xsl:for-each select='$options'>
            <option>
                <xsl:apply-templates select='@* | node()'/>
            </option>
        </xsl:for-each>
        <xsl:apply-templates select='@* | node()'/>
        <xsl:apply-templates select='.' mode='append'/>
</xsl:copy>
    </xsl:template>

    <xsl:template match='option' >
        <xsl:choose>
            <xsl:when test='$options[@key = current()/@key]' />
            <xsl:otherwise><xsl:copy><xsl:apply-templates select='@* | node()'/></xsl:copy></xsl:otherwise>
        </xsl:choose>
    </xsl:template>
</xsl:stylesheet>