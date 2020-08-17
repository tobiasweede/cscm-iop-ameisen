Module main

    Sub Main()

        For Each foundFile As String In My.Computer.FileSystem.GetFiles(CurDir() & "\Data\") 'batch load
            Dim NewInstance As New Instance

            NewInstance.LoadInst(foundFile)

            Dim currentSkuOrder = initSolution(NewInstance)
            Dim currentObj = getObjectiveValue(currentSkuOrder, NewInstance)

        Next

    End Sub

    Sub ameisen()
        ' Anzahl NBL = Polynomialkoeffizient, d.h. (Anzahl SKUs - Anzahl Plätze + 1) P 2

    End Sub

    Function initSolution(ByRef instance As Instance) As List(Of Integer)
        Dim skuOrder As New List(Of Integer)
        For index = 1 To instance.SKUs
            skuOrder.Add(index)
        Next
        Return skuOrder
    End Function

    Function getObjectiveValue(ByVal skuOrder As List(Of Integer), ByRef NewInstance As Instance) As Integer
        Dim obj As Integer ' ZFW

        Return obj
    End Function


End Module
