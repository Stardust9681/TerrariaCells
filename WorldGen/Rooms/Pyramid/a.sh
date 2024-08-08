for i in $(ls); do
	mv $i $(echo $i | tr '-' '_')
done
