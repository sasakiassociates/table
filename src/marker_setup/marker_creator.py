import PIL.Image as PImage
from reportlab.lib.pagesizes import letter
from reportlab.platypus import SimpleDocTemplate, Image, Table, TableStyle
from reportlab.lib import units

class Creator():

    def create_pdf(self, input_images, output_pdf, image_size, margin_size):
        doc = SimpleDocTemplate(output_pdf, pagesize=letter)

        image_size = self.inches_to_points(image_size)
        margin_size = self.inches_to_points(margin_size)
        print(image_size)

        column = int(doc.width / (image_size + margin_size))
        print(column)
        story = []
        row = []
        
        table_style = TableStyle([
            ('GRID', (0, 0), (-1, -1), 0.5, (0, 0, 0)),
            ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
            ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),  # Center vertically
        ])

        for image_path in input_images:
            img = PImage.open(image_path)
            img_width, img_height = img.size

            aspect_ratio = img_width / img_height

            if img_width > img_height:
                new_width = image_size
                new_height = int(new_width * aspect_ratio)
            else:
                new_height = image_size
                new_width = int(new_height / aspect_ratio)

            image = Image(image_path, width=new_width, height=new_height)

            row.append(image)

            if len(row) == column:
                table = Table([row], colWidths=image_size + margin_size, rowHeights=image_size + margin_size, vAlign="CENTER")
                table.setStyle(table_style)
                story.append(table)
                row = []

            # Add image to the PDF with margin
            # story.append(Image(image_path, width=img_width, height=img_height))

        if len(row) > 0:
            table = Table([row], colWidths=image_size + margin_size, rowHeights=image_size + margin_size, vAlign="CENTER")
            table.setStyle(table_style)
            story.append(table)

        doc.build(story)

    def mm_to_points(self, mm):
        return mm * 2.83465
    
    def inches_to_points(self, inches):
        return int(inches * 72)