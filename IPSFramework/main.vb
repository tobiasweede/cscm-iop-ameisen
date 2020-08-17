Module main

    Sub Main()

        For Each foundFile As String In My.Computer.FileSystem.GetFiles(CurDir() & "\Data\") 'batch load
            Dim NewInstance As New Instance
            NewInstance.LoadInst(foundFile)
            Console.WriteLine("Instance:" + NewInstance.Name)
            Console.WriteLine("Ameisen...")
            ameisen(NewInstance, 10)
        Next
        Console.WriteLine("Press any key...")
        Console.ReadLine()
    End Sub

    Sub ameisen(ByRef NewInstance As Instance, ByVal maxIter As Integer)
        Dim zz As New Random() ' Random generator for monte carlo
        Dim currentOrderSequence = initSolution(NewInstance)
        Dim currentObj = getObjectiveValue(currentOrderSequence, NewInstance)
        Console.WriteLine("Initial sequence:")
        printSequence(currentOrderSequence)
        Console.WriteLine("Initial objective value:" + currentObj.ToString)
        Dim bestOrderSequence(NewInstance.PickingOrders - 1) As Integer ' Track best found solution
        Array.Copy(currentOrderSequence, bestOrderSequence, NewInstance.PickingOrders - 1) ' Deep copy current order sequence
        Dim bestObj = currentObj
        Array.Copy(currentOrderSequence, bestOrderSequence, NewInstance.PickingOrders - 1)
        ' Init pheromonmatrix with small value
        Dim pheromonmatrix(NewInstance.SKUs - 1, NewInstance.SKUs - 1) As Double
        For i = 0 To NewInstance.SKUs - 1
            For j = 0 To NewInstance.SKUs - 1
                pheromonmatrix(i, j) = 0.1
            Next
        Next
        For iterIndex = 1 To maxIter
            'Calculate neighborhood
            ' Amount of neighbor sequences = Polynomial coefficient, i.e. (SKUs - PickerPlaces + 1) P 2
            Dim neighborhoodSequences As New List(Of Array)
            Dim neighborhoodObjectives As New List(Of Integer)
            For firstOrder = 0 To NewInstance.PickingCapacity - 1 ' Permute served with free orders
                For secondOrder = NewInstance.PickingCapacity To NewInstance.PickingOrders - 1
                    Dim neighborOrderSequence(NewInstance.PickingOrders - 1) As Integer
                    ' Deep copy order sequence array and swap two orders
                    Array.Copy(currentOrderSequence, neighborOrderSequence, NewInstance.PickingOrders - 1)
                    neighborOrderSequence(firstOrder) = currentOrderSequence(secondOrder)
                    neighborOrderSequence(secondOrder) = currentOrderSequence(firstOrder)
                    neighborhoodSequences.Add(neighborOrderSequence)
                    Dim tmpObj As Integer = getObjectiveValue(neighborOrderSequence, NewInstance)
                    neighborhoodObjectives.Add(tmpObj)
                    If tmpObj < bestObj Then ' Check if new best sequence is found
                        Array.Copy(neighborOrderSequence, bestOrderSequence, NewInstance.PickingOrders - 1) ' Deep copy current order sequence
                        bestObj = tmpObj
                    End If
                Next
            Next
            For firstOrder = NewInstance.PickingCapacity To NewInstance.PickingOrders - 1 ' Permute free orders with themselves
                For secondOrder = firstOrder + 1 To NewInstance.PickingOrders - 1
                    Dim neighborOrderSequence(NewInstance.PickingOrders - 1) As Integer
                    ' Deep copy order sequence array and swap two orders
                    Array.Copy(currentOrderSequence, neighborOrderSequence, NewInstance.PickingOrders - 1)
                    neighborOrderSequence(firstOrder) = currentOrderSequence(secondOrder)
                    neighborOrderSequence(secondOrder) = currentOrderSequence(firstOrder)
                    neighborhoodSequences.Add(neighborOrderSequence)
                    neighborhoodObjectives.Add(getObjectiveValue(neighborOrderSequence, NewInstance))
                Next
            Next
            ' Calculate weighted objective values 
            Dim neighborhoodWeightedObjectives As New List(Of Double)
            For objectiveIndex = 0 To neighborhoodObjectives.Count() - 1
                neighborhoodWeightedObjectives.Add(getWeightedObjectiveValue(neighborhoodObjectives(objectiveIndex),
                                                                             neighborhoodSequences(objectiveIndex),
                                                                             pheromonmatrix))
            Next
            ' Choose sequence via monte carlo / routlette wheel selection
            Dim neighborhoodProbabilities As New Dictionary(Of Integer, Double)
            Dim sumWeightedObjectives As Double = neighborhoodObjectives.Sum
            For objectiveIndex = 0 To neighborhoodObjectives.Count() - 1
                ' Be careful to take the couter probabilities as we minimize the amount of sku cycles
                neighborhoodProbabilities.Add(objectiveIndex, 1 - (neighborhoodWeightedObjectives(objectiveIndex) / sumWeightedObjectives))
            Next
            Dim rand As Double = zz.NextDouble()
            Dim pCumm As Double = 0
            For Each sequenceIdx In neighborhoodProbabilities.Keys
                pCumm += neighborhoodProbabilities.Item(sequenceIdx)
                If rand < pCumm Then ' Pick sequence
                    ' Deep copy order sequence array and swap two orders
                    Array.Copy(neighborhoodSequences(sequenceIdx), currentOrderSequence, NewInstance.PickingOrders - 1)
                    currentObj = neighborhoodObjectives(sequenceIdx)
                    Exit For
                End If
            Next
        Next
        Console.WriteLine("Best sequence:")
        printSequence(bestOrderSequence)
        Console.WriteLine("Initial objective value:" + bestObj.ToString)
    End Sub

    Sub printSequence(ByRef seq As Array)
        For i = 0 To seq.Length - 1
            Console.Write(seq(i).ToString + " ")
        Next
        Console.WriteLine()
    End Sub

    Function initSolution(ByRef NewInstance As Instance) As Array
        ' Start solution with lexikographic order
        Dim orderSequence(NewInstance.PickingOrders - 1) As Integer
        For index = 0 To NewInstance.PickingOrders - 1
            orderSequence(index) = index
        Next
        Return orderSequence
    End Function

    Function getWeightedObjectiveValue(ByVal obj As Integer, ByRef seq As Array, ByRef pheromonmatrix As Array) As Double
        ' Collect the weights for the path and return the weighted objective value
        Dim weight As Double = 0
        For i = 0 To seq.Length() - 2
            weight = weight + pheromonmatrix(seq(i), seq(i + 1))
        Next
        Return weight * obj
    End Function

    Function getObjectiveValue(ByRef orderSequence() As Integer, ByRef NewInstance As Instance) As Integer
        Dim obj As Integer ' ZFW
        Dim activeOrders As New List(Of List(Of Integer))
        Dim nextOrder As Integer ' Save index of next order to process
        For nextOrder = 0 To NewInstance.PickingCapacity - 1 ' Copy first orders to processing list
            Dim currentOrder As New List(Of Integer)
            For Each sku In NewInstance.Order(orderSequence(nextOrder)).OrderSKU
                currentOrder.Add(sku)
            Next
            activeOrders.Add(currentOrder)
        Next
        While Not activeOrders.Count() = 0
            'Find most common SKU
            Dim skuCount As New Dictionary(Of Integer, Integer)
            For Each order In activeOrders
                For Each item In order
                    Dim currentCount As Integer = 0
                    skuCount.TryGetValue(item, currentCount)
                    skuCount(item) = currentCount + 1
                Next
            Next
            Dim mostCommonSku As Integer = skuCount.Aggregate(Function(ByVal l, ByVal r) IIf(l.Value > r.Value, l, r)).Key ' Very beatuiful way to determin the key of the most common SKU
            ' Proccess most common SKU
            For Each order In activeOrders
                If order.Contains(mostCommonSku) Then
                    order.RemoveAll(Function(i) i = mostCommonSku)
                End If
            Next
            obj = obj + 1 ' processed SKU counts as 1
            activeOrders.RemoveAll(Function(item) item.Count() = 0) ' Check for finished orders and remove them
            While activeOrders.Count() < NewInstance.PickingCapacity And nextOrder < NewInstance.PickingOrders ' Add new orders if space is available
                Dim currentOrder As New List(Of Integer)
                For Each sku In NewInstance.Order(nextOrder).OrderSKU
                    currentOrder.Add(sku)
                Next
                activeOrders.Add(currentOrder)
                nextOrder = nextOrder + 1
            End While
        End While
        Return obj
    End Function

End Module