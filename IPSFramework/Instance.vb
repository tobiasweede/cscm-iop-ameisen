Imports System.Xml

Public Class Instance
    Public Structure orders 'structure for customer orders
        Public ID As Short 'self-explanatory
        Public OrderSKU() As Integer 'SKU per customer order 
    End Structure

    Public Name As String 'ID of instance
    Public SKUs As Integer 'number of different SKU
    Public PickingOrders As Integer 'number of customer orders

    Public Tours As Integer 'max number of tours
    Public PickingCapacity As Integer 'self-explanatory
    Public Order() As orders 'customer orders
    Private xmlpath As String = "" 'loding path


    Sub LoadInst(ByVal loadPath As String)

        ReDim Me.Order(0)

        Me.xmlpath = loadPath

        Dim reader As System.IO.StreamReader
        reader = New System.IO.StreamReader( _
        Me.xmlpath, _
        System.Text.Encoding.Unicode, _
        False)


        'xmlObjekte 
        Dim xmlDoc As XmlDocument
        Dim xmlRoot As XmlNode
        Dim xmlNodelist1 As XmlNodeList
        Dim xmlNodelist2 As XmlNodeList
        Dim xmlNodelist3 As XmlNodeList
        Dim indexOrder As Short = 0
        Dim indexOrderProd As Short = 0

        xmlDoc = New XmlDocument()
        xmlDoc.Load(Me.xmlpath)

        xmlRoot = xmlDoc.DocumentElement()

        For Each xmlattribute In xmlRoot.Attributes '
            Me.Name = xmlattribute.value
        Next

        xmlNodelist1 = xmlRoot.ChildNodes

        For Each node1 As XmlNode In xmlNodelist1

            If node1.Name = "descriptor" Then
                xmlNodelist2 = node1.ChildNodes
                For Each node1Undernode As XmlNode In xmlNodelist2
                    If node1Undernode.Name = "CountProducts" Then
                        For Each xmlattribute In node1Undernode.Attributes '
                            Me.SKUs = CInt(xmlattribute.value)
                        Next
                    ElseIf node1Undernode.Name = "CountOrders" Then
                        For Each xmlattribute In node1Undernode.Attributes '
                            Me.PickingOrders = CInt(xmlattribute.value)
                        Next
                    ElseIf node1Undernode.Name = "Capacity" Then
                        For Each xmlattribute In node1Undernode.Attributes '
                            Me.PickingCapacity = CInt(xmlattribute.value)
                        Next
                    ElseIf node1Undernode.Name = "MaxTours" Then
                        For Each xmlattribute In node1Undernode.Attributes '
                            Me.Tours = CInt(xmlattribute.value)
                        Next
                    End If
                Next
            ElseIf node1.Name = "Orders" Then
                xmlNodelist2 = node1.ChildNodes
                For Each node1Undernode As XmlNode In xmlNodelist2
                    ReDim Preserve Me.Order(indexOrder)
                    Me.Order(indexOrder).ID = indexOrder
                    indexOrderProd = 0
                    xmlNodelist3 = node1Undernode.ChildNodes
                    For Each nUndernode As XmlNode In xmlNodelist3
                        ReDim Preserve Me.Order(indexOrder).OrderSKU(indexOrderProd)

                        For Each xmlattribute In nUndernode.Attributes '
                            Me.Order(indexOrder).OrderSKU(indexOrderProd) = CInt(xmlattribute.value)
                        Next
                        indexOrderProd += 1
                    Next
                    indexOrder += 1
                Next
            End If
        Next

    End Sub
End Class
