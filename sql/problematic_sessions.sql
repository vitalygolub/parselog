/****** Script for SelectTopNRows command from SSMS  ******/
--SELECT session, min(returnurl), max(returnurl) 
--  FROM [test].[dbo].[logdata]
--group by session having min(returnurl)<>max(returnurl);
;with a as 
(
select *, row_number() over (partition by session order by rownumber) rn from logdata 
)
, b as (select * from a where not (rn=1 and returnurl='') )
select session, max(returnurl), min(returnurl), min(b.timeofday) from b group by session having max(returnurl)<>min(returnurl);

--/SSO_WEB/connect/authorize/callback?client_id=myrimi&response_type=code&scope=openid%20profile%20loyalty%20offline_access%20retail%20cart%20recipes&redirect_uri=https%3A%2F%2Fwww.rimi.lt%2Faccount%2Flogin%2Fcallback&country=lt&lang=lt
--/SSO_WEB/connect/authorize/callback?client_id=ecom&response_type=code&scope=openid%20profile%20loyalty%20offline_access%20commerce%20recipes&redirect_uri=https%3A%2F%2Fwww.rimi.lt%2Fe-parduotuve%2Faccount%2Flogin%2Fcallback&country=lt&lang=lt/SSO_WEB/connect/authorize/callback?client_id=ecom&response_type=code&scope=openid%20profile%20loyalty%20offline_access%20commerce%20recipes&redirect_uri=https%3A%2F%2Fwww.rimi.lt%2Fe-parduotuve%2Faccount%2Flogin%2Fcallback&country=lt&lang=lt
--select * from logdata where session='4277ccee-16eb-7db6-f5bc-1f40b020c146' order by rownumber
--select *, len(returnurl) from logdata where session='0835db30-1939-8481-5f2a-cd436802b076' order by rownumber
