DECLARE @tbl_wxml TABLE(rawXML XML)
DECLARE @waterData XML
DECLARE @siteID int

--USGS Site ID
SET @siteID = 14158740

INSERT INTO @tbl_wxml
EXEC UspGetUSGSDataBySite @siteID

SELECT @waterData = 
(
	SELECT TOP 1 rawXML FROM @tbl_wxml
);

WITH XMLNAMESPACES (
	'http://www.opengis.net/waterml/2.0' AS wml2,
	'http://www.opengis.net/gml/3.2' AS gml,
	'http://www.w3.org/1999/xlink' AS xlink,
	'http://www.opengis.net/om/2.0' AS om,
	'http://www.opengis.net/sampling/2.0' AS sa,
	'http://www.opengis.net/samplingSpatial/2.0' AS sams,
	'http://www.opengis.net/swe/2.0' AS swe
)
--INSERT INTO USGS.dbo.Data
SELECT SUBSTRING(wData.value('(@gml:id)[1]','varchar(40)'), 9, 8) AS SiteID
	,CONVERT(int, RIGHT(wData.value('(../../om:observedProperty/@xlink:href)[1]','varchar(50)'), 5)) AS ParameterCode
	,pData.value('(wml2:MeasurementTVP/wml2:time)[1]','datetimeoffset') AS [Time]
	,pData.value('(wml2:MeasurementTVP/wml2:value)[1]','decimal(12,4)') AS [Value]
	,wData.value('(wml2:defaultPointMetadata)[1]','varchar') AS Qualifier
FROM @waterData.nodes('
	/wml2:Collection
	/wml2:observationMember
	/om:OM_Observation
	/om:result
	/wml2:MeasurementTimeseries'
) AS T1(wData)
CROSS APPLY T1.wData.nodes('
	wml2:point'
) AS T2(pData)