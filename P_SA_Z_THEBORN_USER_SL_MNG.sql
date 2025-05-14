IF OBJECT_ID('UP_SA_Z_WTP_VMI_RPT_H_S') IS NOT NULL       
DROP PROCEDURE UP_SA_Z_WTP_VMI_RPT_H_S

GO

/*******************************************                                                                          
**  System : 프로젝트관리                                                                        
**  Sub System : 전용                                            
**  Page  : VMI재고현황  -> 이디야
**  Desc  :  VMI재고현황 헤더 조회                          
**  Return Values                                                                          
**                                                                          
**  작    성    자  :  김 정 열                                                                        
**  작    성    일  :  2015.12.10
**  수    정    자  :                                                          
**  수    정    일  :                                                                 
*********************************************                                                                          
** Change History                                                                       
*********************************************/                                                                      
CREATE PROC [NEOE].[UP_SA_Z_WTP_VMI_RPT_H_S]                                                  
(                                                
		@P_CD_COMPANY	NVARCHAR(7),
		@P_CD_PLANT		NVARCHAR(20),
		@P_DT_FR		NVARCHAR(8),
		@P_DT_TO		NVARCHAR(8),
		@P_CD_ITEM		NVARCHAR(4000),
		@P_CD_SL		NVARCHAR(20)
)                                                
AS                                                
BEGIN  

	WITH ITEM_QT1 AS (SELECT  A2.CD_COMPANY,  
							A2.CD_PLANT,  
							A2.CD_ITEM,  
							SUM(A2.QT_INV) AS QT_DIV_SL  --전일재고
							--0 AS QT_DIV_SO  
					FROM (SELECT A1.CD_PLANT, A1.CD_ITEM, A1.CD_COMPANY, ISNULL(A1.QT_GOOD_INV,0) AS QT_INV         
							FROM MM_OPENQTL A1 LEFT OUTER JOIN MA_PITEM B1  
							ON  A1.CD_COMPANY = B1.CD_COMPANY  
							AND  A1.CD_PLANT = B1.CD_PLANT  
							AND  A1.CD_ITEM = B1.CD_ITEM          
							WHERE A1.CD_COMPANY = @P_CD_COMPANY          
							AND A1.CD_PLANT = @P_CD_PLANT 
							AND	(ISNULL(@P_CD_ITEM, '') = '' OR A1.CD_ITEM IN   (SELECT CD_STR FROM GETTABLEFROMSPLIT(@P_CD_ITEM)))  
							AND	(ISNULL(@P_CD_SL, '') = '' OR A1.CD_SL = @P_CD_SL)   
							AND A1.YM_STANDARD = LEFT(@P_DT_FR, 4) + '00'          
							UNION ALL          
							SELECT A1.CD_PLANT, A1.CD_ITEM, A1.CD_COMPANY,  
								SUM(A1.QT_GOOD_GR + A1.QT_REJECT_GR + A1.QT_INSP_GR + A1.QT_TRANS_GR) - SUM(A1.QT_GOOD_GI + A1.QT_REJECT_GI + A1.QT_INSP_GI + A1.QT_TRANS_GI) QT_INV       
							FROM MM_OHSLINVM    A1   LEFT OUTER JOIN MA_PITEM B1  
							ON  A1.CD_COMPANY = B1.CD_COMPANY  
							AND  A1.CD_PLANT = B1.CD_PLANT  
							AND  A1.CD_ITEM = B1.CD_ITEM    
							WHERE A1.CD_COMPANY = @P_CD_COMPANY          
							AND A1.YM_IO >= LEFT(@P_DT_FR, 4) + '00'          
							AND A1.YM_IO <= LEFT(@P_DT_FR, 6)   
							AND A1.CD_PLANT = @P_CD_PLANT   
							--AND	A1.CD_ITEM =   A.ItemCode  
							AND	(ISNULL(@P_CD_ITEM, '') = '' OR A1.CD_ITEM IN   (SELECT CD_STR FROM GETTABLEFROMSPLIT(@P_CD_ITEM)))  
							AND	(ISNULL(@P_CD_SL, '') = '' OR A1.CD_SL = @P_CD_SL)         
							GROUP BY A1.CD_PLANT , A1.CD_ITEM ,A1.CD_COMPANY         
							UNION ALL          
							SELECT L1.CD_PLANT, L1.CD_ITEM, L1.CD_COMPANY,        
								L1.QT_GOOD_GR - L1.QT_GOOD_GI + L1.QT_REJECT_GR - L1.QT_REJECT_GI + L1.QT_TRANS_GR - L1.QT_TRANS_GI + L1.QT_INSP_GR - L1.QT_INSP_GI AS  QT_INV  
							FROM   MM_OHSLINVD  L1   LEFT OUTER JOIN MA_PITEM B1  
							ON  L1.CD_COMPANY = B1.CD_COMPANY  
							AND  L1.CD_PLANT = B1.CD_PLANT  
							AND  L1.CD_ITEM = B1.CD_ITEM   
							WHERE L1.CD_COMPANY = @P_CD_COMPANY          
							AND L1.CD_PLANT = @P_CD_PLANT   
							--AND	A1.CD_ITEM =   A.ItemCode   
							AND	(ISNULL(@P_CD_ITEM, '') = '' OR L1.CD_ITEM IN   (SELECT CD_STR FROM GETTABLEFROMSPLIT(@P_CD_ITEM)))  
							AND	(ISNULL(@P_CD_SL, '') = '' OR L1.CD_SL = @P_CD_SL)               
							AND L1.DT_IO < @P_DT_FR 
							AND L1.DT_IO > LEFT(@P_DT_FR, 6) + '00') A2  
					GROUP BY A2.CD_COMPANY, A2.CD_PLANT, A2.CD_ITEM),
		ITEM_QT2 AS (SELECT		A.CD_COMPANY,		--회사
								A.CD_PLANT,			--공장
								A.CD_ITEM,			--품목
								SUM(ISNULL(A.IN_QT_IO, 0)) AS IN_QT_IO,		--입고수량(입고, 창고이동 입고 포함)
								SUM(ISNULL(A.OUT_QT_IO, 0)) AS OUT_QT_IO								--출고수량(출고, 창고이동 출고 포함)
					 FROM		(SELECT	A.CD_COMPANY,		--회사
											B.CD_PLANT,			--공장
											B.CD_ITEM,			--품목
											SUM(ISNULL(B.QT_IO, 0)) AS IN_QT_IO,		--입고수량(입고, 창고이동 입고 포함)
											0 AS OUT_QT_IO								--출고수량(출고, 창고이동 출고 포함)
									FROM	MM_QTIOH_VMI A INNER JOIN MM_QTIO_VMI B
									ON		A.CD_COMPANY = B.CD_COMPANY
									AND		A.NO_IO = B.NO_IO
									WHERE	A.CD_COMPANY = @P_CD_COMPANY
									AND		B.DT_IO	BETWEEN @P_DT_FR AND @P_DT_TO
									AND		B.CD_PLANT = @P_CD_PLANT
									AND		(ISNULL(@P_CD_ITEM, '') = '' OR  B.CD_ITEM IN   (SELECT CD_STR FROM GETTABLEFROMSPLIT(@P_CD_ITEM)))
									AND		(ISNULL(@P_CD_SL, '') = '' OR B.CD_SL = @P_CD_SL)
									AND		B.FG_PS = '1'				--입출고구분(1:입고, 2:출고)
									AND		B.CD_QTIOTP IN ('410', '500')			--수불유형(410:입고, 400:출고, 500:창고이동)
									GROUP BY A.CD_COMPANY, B.CD_PLANT, B.CD_ITEM
									UNION ALL
									SELECT	A.CD_COMPANY,		--회사
											B.CD_PLANT,			--공장
											B.CD_ITEM,			--품목
											0 AS IN_QT_IO,		--입고수량(입고, 창고이동 입고 포함)
											SUM(ISNULL(B.QT_IO, 0)) AS OUT_QT_IO								--출고수량(출고, 창고이동 출고 포함)
									FROM	MM_QTIOH_VMI A INNER JOIN MM_QTIO_VMI B
									ON		A.CD_COMPANY = B.CD_COMPANY
									AND		A.NO_IO = B.NO_IO
									WHERE	A.CD_COMPANY = @P_CD_COMPANY
									AND		B.DT_IO	BETWEEN @P_DT_FR AND @P_DT_TO
									AND		B.CD_PLANT = @P_CD_PLANT
									AND		(ISNULL(@P_CD_ITEM, '') = '' OR  B.CD_ITEM IN   (SELECT CD_STR FROM GETTABLEFROMSPLIT(@P_CD_ITEM)))
									AND		(ISNULL(@P_CD_SL, '') = '' OR B.CD_SL = @P_CD_SL)
									AND		B.FG_PS = '2'				--입출고구분(1:입고, 2:출고)
									AND		B.CD_QTIOTP IN ('400', '500')			--수불유형(410:입고, 400:출고, 500:창고이동)
									GROUP BY A.CD_COMPANY, B.CD_PLANT, B.CD_ITEM) A
					GROUP BY A.CD_COMPANY, A.CD_PLANT, A.CD_ITEM
				),
	ITEM	AS (SELECT	 A1.CD_COMPANY, A1.CD_PLANT, A1.CD_ITEM		
				FROM	(SELECT	A.CD_COMPANY, A.CD_PLANT, A.CD_ITEM
						 FROM	ITEM_QT1 A
						 UNION ALL 
						 SELECT	A.CD_COMPANY, A.CD_PLANT, A.CD_ITEM
						 FROM	ITEM_QT2 A) A1
				GROUP BY A1.CD_COMPANY, A1.CD_PLANT, A1.CD_ITEM		 
						 )
	SELECT	A.CD_COMPANY,			--회사
			A.CD_PLANT,				--공장
			A.CD_ITEM,				--품목코드
			D.NM_ITEM,				--품목명
			D.STND_ITEM,			--규격
			E.NM_SYSDEF AS UNIT_IM,	--단위
			--D.UNIT_IM,				--단위
			ISNULL(B.QT_DIV_SL, 0) AS QT_DIV_SL,			--전일재고
			ISNULL(C.IN_QT_IO, 0) AS IN_QT_IO,				--기간중 입고
			ISNULL(C.OUT_QT_IO, 0) AS OUT_QT_IO,			--기간중 출고
			ISNULL(B.QT_DIV_SL, 0) + ISNULL(C.IN_QT_IO, 0) - ISNULL(C.OUT_QT_IO, 0) AS QT_REMAIN		--재고(전일재고 + 기간중 입고 - 기간중 출고)
	FROM	ITEM A LEFT OUTER JOIN ITEM_QT1 B
	ON		A.CD_COMPANY = B.CD_COMPANY
	AND		A.CD_PLANT = B.CD_PLANT
	AND		A.CD_ITEM = B.CD_ITEM
	LEFT OUTER JOIN ITEM_QT2 C
	ON		A.CD_COMPANY = C.CD_COMPANY
	AND		A.CD_PLANT = C.CD_PLANT
	AND		A.CD_ITEM = C.CD_ITEM
	LEFT OUTER JOIN MA_PITEM D
	ON		A.CD_COMPANY = D.CD_COMPANY
	AND		A.CD_PLANT = D.CD_PLANT
	AND		A.CD_ITEM = D.CD_ITEM
	LEFT OUTER JOIN MA_CODEDTL E
	ON		D.CD_COMPANY = E.CD_COMPANY
	AND		D.UNIT_IM = E.CD_SYSDEF
	AND		E.CD_FIELD = 'MA_B000004'
	ORDER BY A.CD_COMPANY, A.CD_PLANT, A.CD_ITEM;
                                 
END 