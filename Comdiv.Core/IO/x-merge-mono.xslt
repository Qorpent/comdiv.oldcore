<?xml version='1.0' encoding='utf-8'?>
<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
    xmlns:msxsl='urn:schemas-microsoft-com:xslt' exclude-result-prefixes='msxsl'
xmlns:cce='comdiv://config/extensions'
                xmlns:m='comdiv://vfs/x-merge'
>
    <xsl:output method='xml' indent='yes'/>
    <xsl:param name='src' />

    <xsl:template match='@* | node()'>
        <xsl:copy>
            <xsl:apply-templates select='@* | node()'/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match='/*'>
      <xsl:copy>
        
        <xsl:choose>
          <xsl:when test='m:pre'>
            <xsl:apply-templates select='m:pre' mode='m'/>
          </xsl:when>
            <xsl:otherwise>
  
            <xsl:copy-of select='$src//m:pre'/>
  
          </xsl:otherwise>
        </xsl:choose>
        
        <xsl:apply-templates select='@* | node()'/>
        <xsl:apply-templates select ='$src/node()'/>
        
        <xsl:choose>
          <xsl:when test='m:post'>
            <xsl:apply-templates select='m:post' mode='m'/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select='$src//m:post'/>
          </xsl:otherwise>
        </xsl:choose>
        
      </xsl:copy>
    </xsl:template>

  <xsl:template match='m:*'></xsl:template>
  <xsl:template match='m:pre' mode='m'>
    <xsl:copy>
      <xsl:apply-templates select='node()'/>
      <xsl:copy-of select='$src//m:pre/*' />
    </xsl:copy>
    
  </xsl:template>
  <xsl:template match='m:post' mode='m'>
    <xsl:copy>
      <xsl:apply-templates select='node()'/>
      <xsl:copy-of select='$src//m:post/*' />
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>