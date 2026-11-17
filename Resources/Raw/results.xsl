<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes"/>

  <xsl:template match="/Results">
    <html>
      <head>
        <meta charset="utf-8"/>
        <title>Наукові роботи кафедри</title>
        <style>
          body { font-family: Segoe UI, Arial, sans-serif; margin: 24px; background:#f5f6fa; }
          h1 { color:#2c3e50; }
          table { border-collapse: collapse; margin-bottom: 18px; width: 100%; max-width: 700px; background:#fff; }
          th { background:#34495e; color:#fff; text-align:left; padding:6px 10px; }
          td { border:1px solid #ddd; padding:6px 10px; }
          td:first-child { font-weight:bold; width:35%; background:#ecf0f1; }
        </style>
      </head>
      <body>
        <h1>Наукові роботи кафедри</h1>
        <p>Знайдено записів: <b><xsl:value-of select="count(Record)"/></b></p>
        <xsl:for-each select="Record">
          <table>
            <tr><th colspan="2">Запис № <xsl:value-of select="position()"/></th></tr>
            <xsl:for-each select="@*">
              <tr><td><xsl:value-of select="name()"/></td><td><xsl:value-of select="."/></td></tr>
            </xsl:for-each>
          </table>
        </xsl:for-each>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
