cd /
cd /apps/mongrel2-v1.11.0
m2sh start -name prod &
python OnUpload.py &
python QueueForProcessingManager.py &
python ProcessData_Q1.py &
python ProcessData_Q2.py &
python ProcessData_Q3.py &
python ProcessData_Q4.py &
python PickupReq.py &
python SystemStatus_prod.py &
