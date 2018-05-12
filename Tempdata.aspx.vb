Imports System.Data.SqlClient

Partial Class Tempdata
    Inherits System.Web.UI.Page

    '  Dim cn As New SqlConnection("Data Source=CPU1305189;Initial Catalog='Temp and Hum';Persist Security Info=True;User ID=wfuser;Password=wfpass")
    Dim cn As New SqlConnection("Data Source=PPSNOTEBOOK\SQLEXPRESS;Initial Catalog='Temp and Hum';Integrated Security=True")

    Dim cmd As New Data.SqlClient.SqlCommand
    Dim reader As Data.SqlClient.SqlDataReader

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim SID, dsp_rec As String
        Dim TempC, Hum As Decimal
        Dim tempC_Upper, tempC_Lower, hum_Upper, hum_Lower As Decimal
        Dim Dsp_interval, Rec_interval, opt As Integer
        Dim sts As Integer = 0
        'default conf value
        tempC_Upper = 35.0
        tempC_Lower = 25.0
        hum_Upper = 70.0
        hum_Lower = 40.0
        Dsp_interval = 10
        Rec_interval = 900
        opt = 0



        Try
            cmd.CommandType = System.Data.CommandType.Text
            cmd.Connection = cn

            SID = sender.Request.QueryString("SID")
            TempC = sender.Request.QueryString("Temperature")
            Hum = sender.Request.QueryString("Humidity")
            dsp_rec = sender.Request.QueryString("dsp_rec")
            'Test Data
            'SID = 1001
            'TempC = 26.5
            'Hum = 60.2
            'dsp_rec = "rec"


            'Check Sensor id is exist in sensor_conf, if doesn't exist  add the id and conf data
            cn.Open()
            cmd.CommandText = "SELECT COUNT(*) FROM  sensor_conf WHERE sensorID =" & SID
            Dim datrslt As Integer = cmd.ExecuteScalar()
            If Convert.ToInt32(datrslt) = 0 Then 'doesn't exist  

                cmd.CommandText = "Insert into sensor_conf values('" & SID & "','Name_" & SID & "','SomeWhere'," & tempC_Upper & "," & tempC_Lower & "," & hum_Upper & "," & hum_Lower & "," & Dsp_interval & "," & Rec_interval & "," & opt & " );"   'Insert Defaut setting
                cmd.ExecuteNonQuery()
            Else 'Read conf data
                cmd.CommandText = "SELECT SensorID, TempC_Upper,  TempC_Lower, Hum_Upper, Hum_lower, Dsp_interval, Rec_interval, Opt FROM sensor_conf WHERE SensorID='" & SID & "';"
                reader = cmd.ExecuteReader()
                While reader.Read()
                    SID = reader("SensorID")
                    tempC_Upper = reader("TempC_Upper")
                    tempC_Lower = reader("TempC_Lower")
                    hum_Upper = reader("Hum_Upper")
                    hum_Lower = reader("Hum_Lower")
                    Dsp_interval = reader("dsp_interval")
                    Rec_interval = reader("rec_interval")
                    opt = reader("opt")
                End While
                cn.Close()
            End If



            'Compare measurement to spec
            If (TempC > tempC_Upper) Then sts = sts + 1
            If (TempC < tempC_Lower) Then sts = sts + 2
            If (Hum > hum_Upper) Then sts = sts + 4
            If (Hum < hum_Lower) Then sts = sts + 8

            'Update Current temp and humidity data for view
            cn.Open()
            cmd.CommandText = "IF NOT EXISTS (SELECT * FROM curr_temphum WHERE sensorID='" & SID & "')" _
            & " insert into  curr_temphum(SensorID, TempC, Hum, lastupdate,status) values('" & SID & "' , '" & TempC & "' , '" & Hum & "',  CURRENT_TIMESTAMP," & sts & ")" _
            & " ELSE BEGIN UPDATE curr_temphum SET TempC='" & TempC & "',Hum='" & Hum & "',lastupdate= CURRENT_TIMESTAMP ,Status=" & sts & " WHERE sensorID='" & SID & "' END"
            cmd.ExecuteNonQuery()
            'confirm insert result
            datrslt = CInt(cmd.ExecuteScalar())
            cn.Close()

                'Record temp and Humidity data
            If (dsp_rec = "rec") Then
                cn.Open()
                cmd.CommandText = "insert into Data_temphum(SensorID, TempC, Hum, Date_rec,Status) values('" & SID & "' , '" & TempC & "' , '" & Hum & "' ,  CURRENT_TIMESTAMP ," & sts & ");"
                cmd.ExecuteNonQuery()
                'confirm insert result
                datrslt = CInt(cmd.ExecuteScalar())
                cn.Close()
            End If
                Response.Write("<sts>" & sts & "</sts>")
                Response.Write("<dspIntv>" & Dsp_interval & "</dspIntv>")
                Response.Write("<recIntv>" & Rec_interval & "</recIntv>")
                Response.Write("<datWrt>" & datrslt & "</datWrt>")
        Catch ex As Exception
            'MsgBox(ex.Message)
            Response.Write(ex.Message)
        Finally
            cn.Close()

        End Try


    End Sub
End Class
